using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Domain.BackendRoom;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IBackendRoomService
{
    Task<Result<BackendRoomSnapshot>> GetRoomAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomNote>> CreateNoteAsync(
        Guid userId,
        CreateBackendRoomNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomNote>> UpdateNoteAsync(
        Guid userId,
        Guid noteId,
        UpdateBackendRoomNoteRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomActionResult>> DeleteNoteAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomDrawing>> SaveDrawingAsync(
        Guid userId,
        SaveBackendRoomDrawingRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<BackendRoomCommunityPost>>> GetCommunityPostsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomCommunityPost>> ShareDrawingAsync(
        Guid userId,
        ShareBackendRoomDrawingRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomLikeResult>> ToggleCommunityPostLikeAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default);

    Task<Result<BackendRoomActionResult>> DeleteCommunityPostAsync(
        Guid userId,
        Guid postId,
        CancellationToken cancellationToken = default);
}
