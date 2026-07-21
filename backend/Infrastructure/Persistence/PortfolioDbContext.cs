using CaioMatheusDev.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaioMatheusDev.Api.Infrastructure.Persistence;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<AuthUserEntity> AuthUsers => Set<AuthUserEntity>();

    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

    public DbSet<BackendRoomNoteEntity> BackendRoomNotes => Set<BackendRoomNoteEntity>();

    public DbSet<BackendRoomDrawingEntity> BackendRoomDrawings => Set<BackendRoomDrawingEntity>();

    public DbSet<BackendRoomCommunityPostEntity> BackendRoomCommunityPosts => Set<BackendRoomCommunityPostEntity>();

    public DbSet<BackendRoomCommunityPostLikeEntity> BackendRoomCommunityPostLikes => Set<BackendRoomCommunityPostLikeEntity>();

    public DbSet<WalletEntity> Wallets => Set<WalletEntity>();

    public DbSet<WalletTransferEntity> WalletTransfers => Set<WalletTransferEntity>();

    public DbSet<WalletMovementEntity> WalletMovements => Set<WalletMovementEntity>();

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

        modelBuilder.Entity<BackendRoomNoteEntity>(builder =>
        {
            builder.ToTable("backend_room_notes");
            builder.HasKey(note => note.Id);
            builder.Property(note => note.Content).HasMaxLength(1200).IsRequired();
            builder.Property(note => note.CreatedAt).IsRequired();
            builder.Property(note => note.UpdatedAt).IsRequired();
            builder.HasIndex(note => new { note.UserId, note.UpdatedAt });
            builder.HasOne(note => note.User)
                .WithMany(user => user.BackendRoomNotes)
                .HasForeignKey(note => note.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BackendRoomDrawingEntity>(builder =>
        {
            builder.ToTable("backend_room_drawings");
            builder.HasKey(drawing => drawing.Id);
            builder.Property(drawing => drawing.Name).HasMaxLength(80).IsRequired();
            builder.Property(drawing => drawing.DataUrl).IsRequired();
            builder.Property(drawing => drawing.CreatedAt).IsRequired();
            builder.Property(drawing => drawing.UpdatedAt).IsRequired();
            builder.HasIndex(drawing => drawing.UserId).IsUnique();
            builder.HasOne(drawing => drawing.User)
                .WithOne(user => user.BackendRoomDrawing)
                .HasForeignKey<BackendRoomDrawingEntity>(drawing => drawing.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BackendRoomCommunityPostEntity>(builder =>
        {
            builder.ToTable("backend_room_community_posts");
            builder.HasKey(post => post.Id);
            builder.Property(post => post.AuthorName).HasMaxLength(120).IsRequired();
            builder.Property(post => post.Caption).HasMaxLength(160).IsRequired();
            builder.Property(post => post.DataUrl).IsRequired();
            builder.Property(post => post.CreatedAt).IsRequired();
            builder.Property(post => post.ExpiresAt).IsRequired();
            builder.HasIndex(post => post.CreatedAt);
            builder.HasIndex(post => post.ExpiresAt);
            builder.HasIndex(post => new { post.UserId, post.CreatedAt });
            builder.HasOne(post => post.User)
                .WithMany(user => user.BackendRoomCommunityPosts)
                .HasForeignKey(post => post.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BackendRoomCommunityPostLikeEntity>(builder =>
        {
            builder.ToTable("backend_room_community_post_likes");
            builder.HasKey(like => new { like.PostId, like.UserId });
            builder.Property(like => like.CreatedAt).IsRequired();
            builder.HasIndex(like => like.UserId);
            builder.HasOne(like => like.Post)
                .WithMany(post => post.Likes)
                .HasForeignKey(like => like.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(like => like.User)
                .WithMany(user => user.BackendRoomCommunityPostLikes)
                .HasForeignKey(like => like.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WalletEntity>(builder =>
        {
            builder.ToTable("wallets");
            builder.HasKey(wallet => wallet.Id);
            builder.Property(wallet => wallet.Balance).HasColumnType("numeric(18,2)").IsRequired();
            builder.Property(wallet => wallet.Currency).HasMaxLength(3).IsRequired();
            builder.Property(wallet => wallet.CreatedAt).IsRequired();
            builder.Property(wallet => wallet.UpdatedAt).IsRequired();
            builder.HasIndex(wallet => wallet.UserId).IsUnique();
            builder.HasOne(wallet => wallet.User)
                .WithOne(user => user.Wallet)
                .HasForeignKey<WalletEntity>(wallet => wallet.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WalletTransferEntity>(builder =>
        {
            builder.ToTable("wallet_transfers");
            builder.HasKey(transfer => transfer.Id);
            builder.Property(transfer => transfer.Amount).HasColumnType("numeric(18,2)").IsRequired();
            builder.Property(transfer => transfer.Description).HasMaxLength(160).IsRequired();
            builder.Property(transfer => transfer.Status).HasMaxLength(30).IsRequired();
            builder.Property(transfer => transfer.CreatedAt).IsRequired();
            builder.HasIndex(transfer => transfer.FromWalletId);
            builder.HasIndex(transfer => transfer.ToWalletId);
            builder.HasIndex(transfer => transfer.CreatedAt);
            builder.HasOne(transfer => transfer.FromWallet)
                .WithMany(wallet => wallet.OutgoingTransfers)
                .HasForeignKey(transfer => transfer.FromWalletId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(transfer => transfer.ToWallet)
                .WithMany(wallet => wallet.IncomingTransfers)
                .HasForeignKey(transfer => transfer.ToWalletId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WalletMovementEntity>(builder =>
        {
            builder.ToTable("wallet_movements");
            builder.HasKey(movement => movement.Id);
            builder.Property(movement => movement.Type).HasMaxLength(20).IsRequired();
            builder.Property(movement => movement.Amount).HasColumnType("numeric(18,2)").IsRequired();
            builder.Property(movement => movement.Description).HasMaxLength(160).IsRequired();
            builder.Property(movement => movement.CreatedAt).IsRequired();
            builder.HasIndex(movement => new { movement.WalletId, movement.CreatedAt });
            builder.HasOne(movement => movement.Wallet)
                .WithMany(wallet => wallet.Movements)
                .HasForeignKey(movement => movement.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(movement => movement.Transfer)
                .WithMany(transfer => transfer.Movements)
                .HasForeignKey(movement => movement.TransferId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

