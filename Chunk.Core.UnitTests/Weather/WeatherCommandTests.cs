using Chunk.Core.Weather;
using Moq;

namespace Chunk.Core.UnitTests.Weather;

public class WeatherCommandTests
{
    private readonly Mock<IWeatherService> _weatherService = new();
    private readonly WeatherCommand _command;

    public WeatherCommandTests()
    {
        _command = new WeatherCommand(_weatherService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_NoArgs_ReturnsUsage()
    {
        var reply = await _command.ExecuteAsync(" ", CancellationToken.None);

        Assert.StartsWith("Usage:", reply);
        _weatherService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_UnknownLocation_SaysSo()
    {
        _weatherService.Setup(s => s.GetWeatherAsync("Nowhereville", It.IsAny<CancellationToken>()))
            .ReturnsAsync((WeatherReport?)null);

        var reply = await _command.ExecuteAsync("Nowhereville", CancellationToken.None);

        Assert.Contains("couldn't find", reply);
    }

    [Fact]
    public async Task ExecuteAsync_FormatsReport()
    {
        var report = new WeatherReport(
            new WeatherLocation("Cohasset", "Massachusetts", "US", 42.24, -70.80),
            new CurrentConditions(50.9, 45.1, 68, 0, 8.2, 20.6),
            [
                new DailyForecast(new DateOnly(2026, 11, 9), 3, 54.8, 43.7, 4),
                new DailyForecast(new DateOnly(2026, 11, 10), 61, 51.0, 39.3, null)
            ]);
        _weatherService.Setup(s => s.GetWeatherAsync("02025", It.IsAny<CancellationToken>())).ReturnsAsync(report);

        var reply = await _command.ExecuteAsync("02025", CancellationToken.None);

        Assert.Contains("*Weather for Cohasset, Massachusetts*", reply);
        Assert.Contains(":sunny: Clear, *51°F* (feels like 45°F)", reply);
        Assert.Contains("Humidity 68% · Wind 8 mph, gusts 21 mph", reply);
        Assert.Contains("`Mon` :cloud: Overcast, 55° / 44°, 4% precip", reply);
        Assert.Contains("`Tue` :rain_cloud: Light rain, 51° / 39°", reply);
    }
}
