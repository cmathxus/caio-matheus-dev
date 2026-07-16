using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IAuthUserStore
{
    Task<AuthUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<AuthUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(AuthUser user, CancellationToken cancellationToken = default);

    Task UpdatePasswordAsync(
        Guid userId,
        string passwordHash,
        string passwordSalt,
        CancellationToken cancellationToken = default);
}
