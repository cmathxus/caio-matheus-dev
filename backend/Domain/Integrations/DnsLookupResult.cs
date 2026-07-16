namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record DnsLookupResult(
    string Domain,
    string RecordType,
    IReadOnlyCollection<DnsAnswer> Answers);

public sealed record DnsAnswer(string Name, string Type, int Ttl, string Data);
