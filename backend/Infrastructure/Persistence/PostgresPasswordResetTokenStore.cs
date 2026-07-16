using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;
using CaioMatheusDev.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaioMatheusDev.Api.Infrastructure.Persistence;

public sealed class PostgresPasswordResetTokenStore(PortfolioDbContext dbContext) : IPasswordResetTokenStore
{
    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        dbContext.PasswordResetTokens.Add(new PasswordResetTokenEntity
        {
            Id = token.Id,
            UserId = token.UserId,
            TokenHash = token.TokenHash,
            CreatedAt = token.CreatedAt,
            ExpiresAt = token.ExpiresAt,
            UsedAt = token.UsedAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasswordResetToken?> FindValidAsync(
        Guid userId,
        string tokenHash,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.PasswordResetTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId)
            .Where(token => token.TokenHash == tokenHash)
            .Where(token => token.UsedAt == null)
            .Where(token => token.ExpiresAt > now)
            .OrderByDescending(token => token.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return entity?.ToDomain();
    }

    public async Task MarkUsedAsync(Guid tokenId, DateTimeOffset usedAt, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.PasswordResetTokens.FirstOrDefaultAsync(token => token.Id == tokenId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.UsedAt = usedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

file static class PasswordResetTokenEntityMapper
{
    public static PasswordResetToken ToDomain(this PasswordResetTokenEntity entity) =>
        new(
            entity.Id,
            entity.UserId,
            entity.TokenHash,
            entity.CreatedAt,
            entity.ExpiresAt,
            entity.UsedAt);
}

