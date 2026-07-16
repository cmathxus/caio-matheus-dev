namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record IntegrationEnvelope<T>(T Items, bool FromCache, DateTimeOffset FetchedAt);
