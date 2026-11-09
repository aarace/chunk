using Chunk.Core.Commands;
using Chunk.Worker.Slack;
using Moq;
using SlackNet;
using SlackNet.Events;
using SlackNet.WebApi;

namespace Chunk.Worker.UnitTests.Slack;

public class ChatEventHandlerTests
{
    private readonly Mock<ICommandService> _commands = new();
    private readonly Mock<IChatApi> _chat = new();
    private readonly ChatEventHandler _handler;

    public ChatEventHandlerTests()
    {
        var slack = new Mock<ISlackApiClient>();
        slack.SetupGet(s => s.Chat).Returns(_chat.Object);
        _handler = new ChatEventHandler(_commands.Object, slack.Object);
    }

    [Fact]
    public async Task Handle_Message_PostsReplyInSameChannelAndThread()
    {
        _commands.Setup(c => c.HandleAsync("!weather 02025", It.IsAny<CancellationToken>())).ReturnsAsync("sunny");

        await _handler.Handle(new MessageEvent { Text = "!weather 02025", Channel = "C1", ThreadTs = "123.45" });

        _chat.Verify(c => c.PostMessage(
            It.Is<Message>(m => m.Channel == "C1" && m.Text == "sunny" && m.ThreadTs == "123.45"),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Handle_MessageThatIsNotACommand_PostsNothing()
    {
        _commands.Setup(c => c.HandleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        await _handler.Handle(new MessageEvent { Text = "just chatting", Channel = "C1" });

        _chat.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("B123", null)]
    [InlineData(null, "message_changed")]
    public async Task Handle_BotOrSubtypedMessage_IsIgnored(string? botId, string? subtype)
    {
        await _handler.Handle(new MessageEvent { Text = "!weather 02025", Channel = "C1", BotId = botId, Subtype = subtype });

        _commands.VerifyNoOtherCalls();
        _chat.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("<@U0CHUNK> weather 02025", "!weather 02025")]
    [InlineData("<@U0CHUNK> !weather 02025", "!weather 02025")]
    [InlineData("<@U0CHUNK>", "!help")]
    public async Task Handle_LeadingMention_RunsCommandWithPrefix(string text, string expectedCommand)
    {
        _commands.Setup(c => c.HandleAsync(expectedCommand, It.IsAny<CancellationToken>())).ReturnsAsync("ok");

        await _handler.Handle(new AppMention { Text = text, Channel = "C1" });

        _chat.Verify(c => c.PostMessage(It.Is<Message>(m => m.Text == "ok"), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Handle_TrailingMention_IsLeftToMessageHandler()
    {
        await _handler.Handle(new AppMention { Text = "!weather 02025 <@U0CHUNK>", Channel = "C1" });

        _commands.VerifyNoOtherCalls();
    }
}
