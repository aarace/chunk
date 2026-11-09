using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Chunk.Core.Weather;

public partial class WeatherService : IWeatherService
{
    public const string GeocodingClientName = "open-meteo-geocoding";
    public const string ForecastClientName = "open-meteo-forecast";

    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<WeatherReport?> GetWeatherAsync(string location, CancellationToken cancellationToken)
    {
        var place = await GeocodeAsync(location, cancellationToken);
        if (place is null)
        {
            return null;
        }

        var lat = place.Latitude.ToString(CultureInfo.InvariantCulture);
        var lon = place.Longitude.ToString(CultureInfo.InvariantCulture);
        var url = $"v1/forecast?latitude={lat}&longitude={lon}" +
                  "&current=temperature_2m,apparent_temperature,relative_humidity_2m,weather_code,wind_speed_10m,wind_gusts_10m" +
                  "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
                  "&temperature_unit=fahrenheit&wind_speed_unit=mph&timezone=auto&forecast_days=3";

        var forecast = await _httpClientFactory.CreateClient(ForecastClientName)
            .GetFromJsonAsync<ForecastResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Open-Meteo returned an empty forecast.");

        var current = new CurrentConditions(
            forecast.Current.Temperature,
            forecast.Current.ApparentTemperature,
            forecast.Current.RelativeHumidity,
            forecast.Current.WeatherCode,
            forecast.Current.WindSpeed,
            forecast.Current.WindGusts);

        var daily = forecast.Daily.Time
            .Select((date, i) => new DailyForecast(
                DateOnly.Parse(date, CultureInfo.InvariantCulture),
                forecast.Daily.WeatherCode[i],
                forecast.Daily.TemperatureMax[i],
                forecast.Daily.TemperatureMin[i],
                forecast.Daily.PrecipitationProbabilityMax[i]))
            .ToList();

        return new WeatherReport(
            new WeatherLocation(place.Name, place.Admin1, place.CountryCode, place.Latitude, place.Longitude),
            current,
            daily);
    }

    private async Task<GeocodingResult?> GeocodeAsync(string location, CancellationToken cancellationToken)
    {
        var query = $"v1/search?name={Uri.EscapeDataString(location)}&count=1&language=en&format=json";
        // A bare 5-digit ZIP is ambiguous worldwide, so pin it to the US.
        if (UsZipPattern().IsMatch(location))
        {
            query += "&countryCode=US";
        }

        var response = await _httpClientFactory.CreateClient(GeocodingClientName)
            .GetFromJsonAsync<GeocodingResponse>(query, cancellationToken);
        return response?.Results?.FirstOrDefault();
    }

    [GeneratedRegex(@"^\d{5}$")]
    private static partial Regex UsZipPattern();

    private record GeocodingResponse([property: JsonPropertyName("results")] List<GeocodingResult>? Results);

    private record GeocodingResult(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("latitude")] double Latitude,
        [property: JsonPropertyName("longitude")] double Longitude,
        [property: JsonPropertyName("admin1")] string? Admin1,
        [property: JsonPropertyName("country_code")] string? CountryCode);

    private record ForecastResponse(
        [property: JsonPropertyName("current")] CurrentBlock Current,
        [property: JsonPropertyName("daily")] DailyBlock Daily);

    private record CurrentBlock(
        [property: JsonPropertyName("temperature_2m")] double Temperature,
        [property: JsonPropertyName("apparent_temperature")] double ApparentTemperature,
        [property: JsonPropertyName("relative_humidity_2m")] int RelativeHumidity,
        [property: JsonPropertyName("weather_code")] int WeatherCode,
        [property: JsonPropertyName("wind_speed_10m")] double WindSpeed,
        [property: JsonPropertyName("wind_gusts_10m")] double WindGusts);

    private record DailyBlock(
        [property: JsonPropertyName("time")] List<string> Time,
        [property: JsonPropertyName("weather_code")] List<int> WeatherCode,
        [property: JsonPropertyName("temperature_2m_max")] List<double> TemperatureMax,
        [property: JsonPropertyName("temperature_2m_min")] List<double> TemperatureMin,
        [property: JsonPropertyName("precipitation_probability_max")] List<int?> PrecipitationProbabilityMax);
}
