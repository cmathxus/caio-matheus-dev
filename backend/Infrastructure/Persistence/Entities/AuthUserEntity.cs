namespace CaioMatheusDev.Api.Infrastructure.Persistence.Entities;

public sealed class AuthUserEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string PasswordSalt { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<PasswordResetTokenEntity> PasswordResetTokens { get; set; } = [];

    public ICollection<BackendRoomNoteEntity> BackendRoomNotes { get; set; } = [];

    public BackendRoomDrawingEntity? BackendRoomDrawing { get; set; }

    public ICollection<BackendRoomCommunityPostEntity> BackendRoomCommunityPosts { get; set; } = [];

    public ICollection<BackendRoomCommunityPostLikeEntity> BackendRoomCommunityPostLikes { get; set; } = [];
}

