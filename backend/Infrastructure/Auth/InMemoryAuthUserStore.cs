using System.Collections.Concurrent;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Infrastructure.Auth;

public sealed class InMemoryAuthUserStore : IAuthUserStore
{
    private readonly ConcurrentDictionary<string, AuthUser> usersByEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, string> emailByUserId = new();

    public Task<AuthUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        usersByEmail.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task<AuthUser?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!emailByUserId.TryGetValue(id, out var email))
        {
            return Task.FromResult<AuthUser?>(null);
        }

        usersByEmail.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(usersByEmail.ContainsKey(email));

    public Task AddAsync(AuthUser user, CancellationToken cancellationToken = default)
    {
        if (!usersByEmail.TryAdd(user.Email, user))
        {
            throw new InvalidOperationException("User already exists.");
        }

        emailByUserId[user.Id] = user.Email;
        return Task.CompletedTask;
    }

    public Task UpdatePasswordAsync(
        Guid userId,
        string passwordHash,
        string passwordSalt,
        CancellationToken cancellationToken = default)
    {
        if (!emailByUserId.TryGetValue(userId, out var email) ||
            !usersByEmail.TryGetValue(email, out var user))
        {
            return Task.CompletedTask;
        }

        usersByEmail[email] = user with
        {
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        return Task.CompletedTask;
    }
}
