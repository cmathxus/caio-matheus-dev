namespace CaioMatheusDev.Api.Infrastructure.Persistence.Entities;

public sealed class BackendRoomCommunityPostLikeEntity
{
    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public BackendRoomCommunityPostEntity? Post { get; set; }

    public AuthUserEntity? User { get; set; }
}
