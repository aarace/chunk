namespace Chunk.Core.Weather;

public interface IWeatherService
{
    /// <returns>The report, or null when the location can't be found.</returns>
    Task<WeatherReport?> GetWeatherAsync(string location, CancellationToken cancellationToken);
}
