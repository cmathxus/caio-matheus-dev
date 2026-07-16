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
    }
}

