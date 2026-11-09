using Chunk.Core.Commands;
using Chunk.Core.Weather;
using Microsoft.Extensions.DependencyInjection;

namespace Chunk.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChunkCore(this IServiceCollection services)
    {
        services.AddSingleton<ICommandService, CommandService>();

        services.AddHttpClient(WeatherService.GeocodingClientName,
            c => c.BaseAddress = new Uri("https://geocoding-api.open-meteo.com/"));
        services.AddHttpClient(WeatherService.ForecastClientName,
            c => c.BaseAddress = new Uri("https://api.open-meteo.com/"));
        services.AddSingleton<IWeatherService, WeatherService>();

        services.AddSingleton<IBotCommand, WeatherCommand>();

        return services;
    }
}
