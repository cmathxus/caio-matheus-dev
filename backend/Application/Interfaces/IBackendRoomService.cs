using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Domain.BackendRoom;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IBackendRoomService
{
    Task<Result<BackendRoomSnapshot>> GetRoomAsync(
        string authorizationHeader,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomNote>> CreateNoteAsync(
        string authorizationHeader,
        CreateBackendRoomNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomNote>> UpdateNoteAsync(
        string authorizationHeader,
        Guid noteId,
        UpdateBackendRoomNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomActionResult>> DeleteNoteAsync(
        string authorizationHeader,
        Guid noteId,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomDrawing>> SaveDrawingAsync(
        string authorizationHeader,
        SaveBackendRoomDrawingRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<BackendRoomCommunityPost>>> GetCommunityPostsAsync(
        string authorizationHeader,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomCommunityPost>> ShareDrawingAsync(
        string authorizationHeader,
        ShareBackendRoomDrawingRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomLikeResult>> ToggleCommunityPostLikeAsync(
        string authorizationHeader,
        Guid postId,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomActionResult>> DeleteCommunityPostAsync(
        string authorizationHeader,
        Guid postId,
        CancellationToken cancellationToken = default);
}
