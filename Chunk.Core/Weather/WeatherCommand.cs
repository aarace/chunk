using System.Globalization;
using System.Text;
using Chunk.Core.Commands;

namespace Chunk.Core.Weather;

public class WeatherCommand : IBotCommand
{
    private readonly IWeatherService _weatherService;

    public WeatherCommand(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public string Name => "weather";
    public string Usage => "weather <zip or city>";
    public string Description => "current conditions and a 3-day forecast";

    public async Task<string> ExecuteAsync(string args, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            return $"Usage: `{CommandService.Prefix}{Usage}`, e.g. `{CommandService.Prefix}weather 02025`";
        }

        var report = await _weatherService.GetWeatherAsync(args, cancellationToken);
        return report is null
            ? $"I couldn't find a place called \"{args}\"."
            : Format(report);
    }

    private static string Format(WeatherReport report)
    {
        var loc = report.Location;
        var place = string.Join(", ", new[] { loc.Name, loc.Region }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var (emoji, description) = WeatherCodes.Describe(report.Current.WeatherCode);
        var c = report.Current;

        var sb = new StringBuilder();
        sb.Append($"*Weather for {place}*\n");
        sb.Append($"{emoji} {description}, *{c.TemperatureF:0}°F* (feels like {c.FeelsLikeF:0}°F)\n");
        sb.Append($"Humidity {c.HumidityPercent}% · Wind {c.WindMph:0} mph, gusts {c.GustMph:0} mph\n");

        foreach (var day in report.Daily)
        {
            var (dayEmoji, dayDescription) = WeatherCodes.Describe(day.WeatherCode);
            var label = day.Date.ToString("ddd", CultureInfo.InvariantCulture);
            var precip = day.PrecipitationChancePercent is { } p ? $", {p}% precip" : string.Empty;
            sb.Append($"\n`{label}` {dayEmoji} {dayDescription}, {day.HighF:0}° / {day.LowF:0}°{precip}");
        }

        return sb.ToString();
    }
}
