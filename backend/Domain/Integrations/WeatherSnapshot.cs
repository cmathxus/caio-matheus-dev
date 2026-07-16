namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record WeatherSnapshot(
    string City,
    double Latitude,
    double Longitude,
    double TemperatureCelsius,
    double WindSpeedKmH,
    int WeatherCode,
    DateTimeOffset FetchedAt);
