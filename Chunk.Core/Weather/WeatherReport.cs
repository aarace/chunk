namespace Chunk.Core.Weather;

public record WeatherLocation(string Name, string? Region, string? CountryCode, double Latitude, double Longitude);

public record CurrentConditions(
    double TemperatureF,
    double FeelsLikeF,
    int HumidityPercent,
    int WeatherCode,
    double WindMph,
    double GustMph);

public record DailyForecast(DateOnly Date, int WeatherCode, double HighF, double LowF, int? PrecipitationChancePercent);

public record WeatherReport(WeatherLocation Location, CurrentConditions Current, IReadOnlyList<DailyForecast> Daily);
