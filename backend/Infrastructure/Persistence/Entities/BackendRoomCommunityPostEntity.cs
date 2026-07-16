namespace CaioMatheusDev.Api.Infrastructure.Persistence.Entities;

public sealed class BackendRoomCommunityPostEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string Caption { get; set; } = string.Empty;

    public string DataUrl { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public AuthUserEntity? User { get; set; }

    public ICollection<BackendRoomCommunityPostLikeEntity> Likes { get; set; } = [];
}
