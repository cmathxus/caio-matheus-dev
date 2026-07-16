using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;
using CaioMatheusDev.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaioMatheusDev.Api.Infrastructure.Persistence;

public sealed class PostgresAuthUserStore(PortfolioDbContext dbContext) : IAuthUserStore
{
    public async Task<AuthUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.AuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<AuthUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.AuthUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        dbContext.AuthUsers.AnyAsync(user => user.Email == email, cancellationToken);

    public async Task AddAsync(AuthUser user, CancellationToken cancellationToken = default)
    {
        dbContext.AuthUsers.Add(new AuthUserEntity
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt,
            CreatedAt = user.CreatedAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePasswordAsync(
        Guid userId,
        string passwordHash,
        string passwordSalt,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.AuthUsers.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.PasswordHash = passwordHash;
        entity.PasswordSalt = passwordSalt;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

file static class AuthUserEntityMapper
{
    public static AuthUser ToDomain(this AuthUserEntity entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Email,
            entity.PasswordHash,
            entity.PasswordSalt,
            entity.CreatedAt);
}

