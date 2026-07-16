using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IJwtTokenService
{
    AuthSession CreateSession(AuthUser user);

    Result<AuthTokenPayload> Validate(string authorizationHeader);
}

