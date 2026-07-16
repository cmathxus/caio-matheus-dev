using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Domain.BackendRoom;

public sealed record BackendRoomSnapshot(
    AuthUserProfile User,
    IReadOnlyCollection<BackendRoomNote> Notes,
    BackendRoomDrawing? Drawing,
    IReadOnlyCollection<BackendRoomCommunityPost> CommunityPosts,
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

public sealed record BackendRoomCommunityPost(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string Caption,
    string DataUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    int LikesCount,
    bool LikedByCurrentUser);

public sealed record CreateBackendRoomNoteRequest(string Content);

public sealed record UpdateBackendRoomNoteRequest(string Content);

public sealed record SaveBackendRoomDrawingRequest(string Name, string DataUrl);

public sealed record ShareBackendRoomDrawingRequest(string? Caption, string DataUrl);

public sealed record BackendRoomLikeResult(
    Guid PostId,
    int LikesCount,
    bool LikedByCurrentUser);

public sealed record BackendRoomActionResult(string Message);
