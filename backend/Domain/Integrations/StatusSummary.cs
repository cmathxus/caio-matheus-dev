namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record StatusSummary(DateTimeOffset CheckedAt, StatusCheck[] Checks);
