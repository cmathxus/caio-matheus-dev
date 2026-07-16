using CaioMatheusDev.Api.Domain.BackendRoom;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IBackendRoomStore
{
    Task<IReadOnlyCollection<BackendRoomNote>> GetNotesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<BackendRoomNote> AddNoteAsync(
        BackendRoomNote note,
        CancellationToken cancellationToken = default);

    Task<BackendRoomNote?> UpdateNoteAsync(
        Guid userId,
        Guid noteId,
        string content,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteNoteAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken = default);

    Task<BackendRoomDrawing?> GetDrawingAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<BackendRoomDrawing> SaveDrawingAsync(
        BackendRoomDrawing drawing,
        CancellationToken cancellationToken = default);
}
