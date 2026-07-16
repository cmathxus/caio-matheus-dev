using System.Collections.Concurrent;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Infrastructure.Auth;

public sealed class InMemoryPasswordResetTokenStore : IPasswordResetTokenStore
{
    private readonly ConcurrentDictionary<Guid, PasswordResetToken> tokensById = new();

    public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        tokensById[token.Id] = token;
        return Task.CompletedTask;
    }

    public Task<PasswordResetToken?> FindValidAsync(
        Guid userId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var token = tokensById.Values
            .Where(item => item.UserId == userId)
            .Where(item => item.TokenHash == tokenHash)
            .Where(item => item.UsedAt is null)
            .Where(item => item.ExpiresAt > now)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefault();

        return Task.FromResult(token);
    }

    public Task MarkUsedAsync(Guid tokenId, DateTimeOffset usedAt, CancellationToken cancellationToken = default)
    {
        if (tokensById.TryGetValue(tokenId, out var token))
        {
            tokensById[tokenId] = token with { UsedAt = usedAt };
        }

        return Task.CompletedTask;
    }
}

