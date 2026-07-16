using System.Collections.Concurrent;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.BackendRoom;

namespace CaioMatheusDev.Api.Infrastructure.Auth;

public sealed class InMemoryBackendRoomStore : IBackendRoomStore
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, BackendRoomNote>> notesByUserId = new();
    private readonly ConcurrentDictionary<Guid, BackendRoomDrawing> drawingsByUserId = new();

    public Task<IReadOnlyCollection<BackendRoomNote>> GetNotesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notes = notesByUserId.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, BackendRoomNote>());

        return Task.FromResult<IReadOnlyCollection<BackendRoomNote>>(
            notes.Values.OrderByDescending(note => note.UpdatedAt).ToList());
    }

    public Task<BackendRoomNote> AddNoteAsync(
        BackendRoomNote note,
        CancellationToken cancellationToken = default)
    {
        var notes = notesByUserId.GetOrAdd(note.UserId, _ => new ConcurrentDictionary<Guid, BackendRoomNote>());
        notes[note.Id] = note;

        return Task.FromResult(note);
    }

    public Task<BackendRoomNote?> UpdateNoteAsync(
        Guid userId,
        Guid noteId,
        string content,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        var notes = notesByUserId.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, BackendRoomNote>());

        if (!notes.TryGetValue(noteId, out var current))
        {
            return Task.FromResult<BackendRoomNote?>(null);
        }

        var updated = current with { Content = content, UpdatedAt = updatedAt };
        notes[noteId] = updated;

        return Task.FromResult<BackendRoomNote?>(updated);
    }

    public Task<bool> DeleteNoteAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        var notes = notesByUserId.GetOrAdd(userId, _ => new ConcurrentDictionary<Guid, BackendRoomNote>());

        return Task.FromResult(notes.TryRemove(noteId, out _));
    }

    public Task<BackendRoomDrawing?> GetDrawingAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(drawingsByUserId.GetValueOrDefault(userId));

    public Task<BackendRoomDrawing> SaveDrawingAsync(
        BackendRoomDrawing drawing,
        CancellationToken cancellationToken = default)
    {
        drawingsByUserId[drawing.UserId] = drawing;

        return Task.FromResult(drawing);
    }
}
