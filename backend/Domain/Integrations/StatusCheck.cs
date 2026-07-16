using System.Net;

namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record StatusCheck(
    string Name,
    string Url,
    HttpStatusCode StatusCode,
    bool Online,
    long LatencyMs,
    DateTimeOffset CheckedAt);
