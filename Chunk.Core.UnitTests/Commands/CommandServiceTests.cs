using Chunk.Core.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Chunk.Core.UnitTests.Commands;

public class CommandServiceTests
{
    private readonly Mock<IBotCommand> _weather = new();
    private readonly CommandService _service;

    public CommandServiceTests()
    {
        _weather.SetupGet(c => c.Name).Returns("weather");
        _weather.SetupGet(c => c.Usage).Returns("weather <zip or city>");
        _weather.SetupGet(c => c.Description).Returns("the weather");
        _service = new CommandService([_weather.Object], NullLogger<CommandService>.Instance);
    }

    [Theory]
    [InlineData("hello there")]
    [InlineData("!")]
    [InlineData("! weather")]
    [InlineData("")]
    public async Task HandleAsync_NotACommand_ReturnsNull(string text)
    {
        Assert.Null(await _service.HandleAsync(text, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_KnownCommand_PassesArgsAndReturnsReply()
    {
        _weather.Setup(c => c.ExecuteAsync("02025", It.IsAny<CancellationToken>())).ReturnsAsync("sunny");

        var reply = await _service.HandleAsync("  !Weather   02025 ", CancellationToken.None);

        Assert.Equal("sunny", reply);
    }

    [Fact]
    public async Task HandleAsync_NoArgs_PassesEmptyArgs()
    {
        _weather.Setup(c => c.ExecuteAsync("", It.IsAny<CancellationToken>())).ReturnsAsync("usage");

        Assert.Equal("usage", await _service.HandleAsync("!weather", CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_UnknownCommand_SuggestsHelp()
    {
        var reply = await _service.HandleAsync("!bogus", CancellationToken.None);

        Assert.Contains("`!bogus`", reply);
        Assert.Contains("`!help`", reply);
    }

    [Fact]
    public async Task HandleAsync_Help_ListsRegisteredCommands()
    {
        var reply = await _service.HandleAsync("!help", CancellationToken.None);

        Assert.Contains("`!weather <zip or city>`: the weather", reply);
        Assert.Contains("`!help`", reply);
    }

    [Fact]
    public async Task HandleAsync_CommandThrows_ReturnsFriendlyError()
    {
        _weather.Setup(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var reply = await _service.HandleAsync("!weather 02025", CancellationToken.None);

        Assert.Contains("Something went wrong", reply);
    }
}
