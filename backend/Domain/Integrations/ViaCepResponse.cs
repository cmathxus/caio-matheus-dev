namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record ViaCepResponse(
    string Cep,
    string Logradouro,
    string Complemento,
    string Bairro,
    string Localidade,
    string Uf,
    bool Erro);
