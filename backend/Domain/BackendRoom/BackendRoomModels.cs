using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Domain.BackendRoom;

public sealed record BackendRoomSnapshot(
    AuthUserProfile User,
    IReadOnlyCollection<BackendRoomNote> Notes,
    BackendRoomDrawing? Drawing,
    DateTimeOffset LoadedAt);

public sealed record BackendRoomNote(
    Guid Id,
    Guid UserId,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record BackendRoomDrawing(
    Guid Id,
    Guid UserId,
    string Name,
    string DataUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateBackendRoomNoteRequest(string Content);

public sealed record UpdateBackendRoomNoteRequest(string Content);

public sealed record SaveBackendRoomDrawingRequest(string Name, string DataUrl);

public sealed record BackendRoomActionResult(string Message);
