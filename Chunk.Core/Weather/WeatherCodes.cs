namespace Chunk.Core.Weather;

// WMO weather interpretation codes as used by Open-Meteo: https://open-meteo.com/en/docs
public static class WeatherCodes
{
    public static (string Emoji, string Description) Describe(int code) => code switch
    {
        0 => (":sunny:", "Clear"),
        1 => (":mostly_sunny:", "Mostly clear"),
        2 => (":partly_sunny:", "Partly cloudy"),
        3 => (":cloud:", "Overcast"),
        45 or 48 => (":fog:", "Fog"),
        51 or 53 or 55 => (":rain_cloud:", "Drizzle"),
        56 or 57 => (":rain_cloud:", "Freezing drizzle"),
        61 => (":rain_cloud:", "Light rain"),
        63 => (":rain_cloud:", "Rain"),
        65 => (":rain_cloud:", "Heavy rain"),
        66 or 67 => (":rain_cloud:", "Freezing rain"),
        71 => (":snow_cloud:", "Light snow"),
        73 => (":snow_cloud:", "Snow"),
        75 => (":snow_cloud:", "Heavy snow"),
        77 => (":snow_cloud:", "Snow grains"),
        80 or 81 or 82 => (":partly_sunny_rain:", "Rain showers"),
        85 or 86 => (":snow_cloud:", "Snow showers"),
        95 => (":thunder_cloud_and_rain:", "Thunderstorm"),
        96 or 99 => (":thunder_cloud_and_rain:", "Thunderstorm with hail"),
        _ => (":grey_question:", "Unknown")
    };
}
