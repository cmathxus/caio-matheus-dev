using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Integrations;

namespace CaioMatheusDev.Api.Infrastructure.Http;

public sealed class AddressLookupService(IHttpClientFactory httpClientFactory) : IAddressLookupService
{
    public async Task<Result<ViaCepResponse>> LookupCepAsync(
        string cep,
        CancellationToken cancellationToken = default)
    {
        var normalized = new string(cep.Where(char.IsDigit).ToArray());
        if (normalized.Length != 8)
        {
            return Result<ViaCepResponse>.Fail("invalid_cep", "CEP must contain 8 digits.");
        }

        var client = httpClientFactory.CreateClient("default");
        var response = await client.GetFromJsonAsync<ViaCepResponse>(
            $"https://viacep.com.br/ws/{normalized}/json/",
            cancellationToken);

        return response is null || response.Erro
            ? Result<ViaCepResponse>.Fail("cep_not_found", "CEP not found.")
            : Result<ViaCepResponse>.Ok(response);
    }
}
