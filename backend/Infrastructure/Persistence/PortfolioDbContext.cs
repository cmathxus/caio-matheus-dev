using CaioMatheusDev.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaioMatheusDev.Api.Infrastructure.Persistence;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<AuthUserEntity> AuthUsers => Set<AuthUserEntity>();

    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuthUserEntity>(builder =>
        {
            builder.ToTable("auth_users");
            builder.HasKey(user => user.Id);
            builder.Property(user => user.Name).HasMaxLength(120).IsRequired();
            builder.Property(user => user.Email).HasMaxLength(160).IsRequired();
            builder.Property(user => user.PasswordHash).HasMaxLength(256).IsRequired();
            builder.Property(user => user.PasswordSalt).HasMaxLength(128).IsRequired();
            builder.Property(user => user.CreatedAt).IsRequired();
            builder.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<PasswordResetTokenEntity>(builder =>
        {
            builder.ToTable("password_reset_tokens");
            builder.HasKey(token => token.Id);
            builder.Property(token => token.TokenHash).HasMaxLength(128).IsRequired();
            builder.Property(token => token.CreatedAt).IsRequired();
            builder.Property(token => token.ExpiresAt).IsRequired();
            builder.HasIndex(token => token.TokenHash).IsUnique();
            builder.HasIndex(token => new { token.UserId, token.ExpiresAt });
            builder.HasOne(token => token.User)
                .WithMany(user => user.PasswordResetTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

