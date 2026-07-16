using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IPasswordResetTokenStore
{
    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);

    Task<PasswordResetToken?> FindValidAsync(
        Guid userId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task MarkUsedAsync(Guid tokenId, DateTimeOffset usedAt, CancellationToken cancellationToken = default);
}

