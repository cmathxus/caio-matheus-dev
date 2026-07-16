namespace CaioMatheusDev.Api.Infrastructure.Persistence.Entities;

public sealed class BackendRoomDrawingEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DataUrl { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public AuthUserEntity? User { get; set; }
}
