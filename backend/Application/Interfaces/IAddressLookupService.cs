using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Domain.Integrations;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IAddressLookupService
{
    Task<Result<ViaCepResponse>> LookupCepAsync(string cep, CancellationToken cancellationToken = default);
}
