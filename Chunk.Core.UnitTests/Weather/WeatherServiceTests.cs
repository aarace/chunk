using System.Net;
using System.Text;
using Chunk.Core.Weather;
using Moq;

namespace Chunk.Core.UnitTests.Weather;

public class WeatherServiceTests
{
    private const string GeocodeJson = """
        {"results":[{"name":"Cohasset","latitude":42.24177,"longitude":-70.80365,"country_code":"US","admin1":"Massachusetts"}]}
        """;

    private const string ForecastJson = """
        {"current":{"temperature_2m":50.9,"apparent_temperature":45.1,"relative_humidity_2m":68,"weather_code":0,"wind_speed_10m":8.2,"wind_gusts_10m":20.6},
         "daily":{"time":["2026-11-09","2026-11-10"],"weather_code":[3,61],"temperature_2m_max":[54.8,51.0],"temperature_2m_min":[43.7,39.3],"precipitation_probability_max":[4,null]}}
        """;

    private readonly List<Uri> _requests = [];
    private readonly Mock<IHttpClientFactory> _factory = new();

    private void Respond(string clientName, string baseAddress, string json)
    {
        var handler = new StubHandler(json, _requests);
        _factory.Setup(f => f.CreateClient(clientName))
            .Returns(() => new HttpClient(handler) { BaseAddress = new Uri(baseAddress) });
    }

    [Fact]
    public async Task GetWeatherAsync_Zip_GeocodesInUsAndMapsForecast()
    {
        Respond(WeatherService.GeocodingClientName, "https://geo.test/", GeocodeJson);
        Respond(WeatherService.ForecastClientName, "https://forecast.test/", ForecastJson);
        var service = new WeatherService(_factory.Object);

        var report = await service.GetWeatherAsync("02025", CancellationToken.None);

        Assert.NotNull(report);
        Assert.Equal("Cohasset", report.Location.Name);
        Assert.Equal("Massachusetts", report.Location.Region);
        Assert.Equal(50.9, report.Current.TemperatureF);
        Assert.Equal(68, report.Current.HumidityPercent);
        Assert.Equal(2, report.Daily.Count);
        Assert.Equal(new DateOnly(2026, 11, 10), report.Daily[1].Date);
        Assert.Null(report.Daily[1].PrecipitationChancePercent);

        Assert.Contains("name=02025", _requests[0].Query);
        Assert.Contains("countryCode=US", _requests[0].Query);
        Assert.Contains("latitude=42.24177", _requests[1].Query);
        Assert.Contains("longitude=-70.80365", _requests[1].Query);
    }

    [Fact]
    public async Task GetWeatherAsync_CityName_DoesNotPinCountry()
    {
        Respond(WeatherService.GeocodingClientName, "https://geo.test/", GeocodeJson);
        Respond(WeatherService.ForecastClientName, "https://forecast.test/", ForecastJson);
        var service = new WeatherService(_factory.Object);

        await service.GetWeatherAsync("Cohasset", CancellationToken.None);

        Assert.DoesNotContain("countryCode", _requests[0].Query);
    }

    [Fact]
    public async Task GetWeatherAsync_NoGeocodeResults_ReturnsNull()
    {
        Respond(WeatherService.GeocodingClientName, "https://geo.test/", """{"generationtime_ms":0.1}""");
        var service = new WeatherService(_factory.Object);

        Assert.Null(await service.GetWeatherAsync("Nowhereville", CancellationToken.None));
        _factory.Verify(f => f.CreateClient(WeatherService.ForecastClientName), Times.Never);
    }

    private class StubHandler(string json, List<Uri> requests) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            requests.Add(request.RequestUri!);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
