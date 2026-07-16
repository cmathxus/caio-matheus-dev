using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.BackendRoom;
using CaioMatheusDev.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaioMatheusDev.Api.Infrastructure.Persistence;

public sealed class PostgresBackendRoomStore(PortfolioDbContext dbContext) : IBackendRoomStore
{
    public async Task<IReadOnlyCollection<BackendRoomNote>> GetNotesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notes = await dbContext.BackendRoomNotes
            .AsNoTracking()
            .Where(note => note.UserId == userId)
            .OrderByDescending(note => note.UpdatedAt)
            .ToListAsync(cancellationToken);

        return notes.Select(note => note.ToDomain()).ToList();
    }

    public async Task<BackendRoomNote> AddNoteAsync(
        BackendRoomNote note,
        CancellationToken cancellationToken = default)
    {
        dbContext.BackendRoomNotes.Add(note.ToEntity());
        await dbContext.SaveChangesAsync(cancellationToken);

        return note;
    }

    public async Task<BackendRoomNote?> UpdateNoteAsync(
        Guid userId,
        Guid noteId,
        string content,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.BackendRoomNotes
            .FirstOrDefaultAsync(note => note.Id == noteId && note.UserId == userId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Content = content;
        entity.UpdatedAt = updatedAt;
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToDomain();
    }

    public async Task<bool> DeleteNoteAsync(
        Guid userId,
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.BackendRoomNotes
            .FirstOrDefaultAsync(note => note.Id == noteId && note.UserId == userId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        dbContext.BackendRoomNotes.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<BackendRoomDrawing?> GetDrawingAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.BackendRoomDrawings
            .AsNoTracking()
            .FirstOrDefaultAsync(drawing => drawing.UserId == userId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<BackendRoomDrawing> SaveDrawingAsync(
        BackendRoomDrawing drawing,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.BackendRoomDrawings
            .FirstOrDefaultAsync(item => item.UserId == drawing.UserId, cancellationToken);

        if (entity is null)
        {
            dbContext.BackendRoomDrawings.Add(drawing.ToEntity());
        }
        else
        {
            entity.Name = drawing.Name;
            entity.DataUrl = drawing.DataUrl;
            entity.UpdatedAt = drawing.UpdatedAt;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return drawing;
    }
}

file static class BackendRoomEntityMapper
{
    public static BackendRoomNote ToDomain(this BackendRoomNoteEntity entity) =>
        new(entity.Id, entity.UserId, entity.Content, entity.CreatedAt, entity.UpdatedAt);

    public static BackendRoomNoteEntity ToEntity(this BackendRoomNote note) =>
        new()
        {
            Id = note.Id,
            UserId = note.UserId,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };

    public static BackendRoomDrawing ToDomain(this BackendRoomDrawingEntity entity) =>
        new(entity.Id, entity.UserId, entity.Name, entity.DataUrl, entity.CreatedAt, entity.UpdatedAt);

    public static BackendRoomDrawingEntity ToEntity(this BackendRoomDrawing drawing) =>
        new()
        {
            Id = drawing.Id,
            UserId = drawing.UserId,
            Name = drawing.Name,
            DataUrl = drawing.DataUrl,
            CreatedAt = drawing.CreatedAt,
            UpdatedAt = drawing.UpdatedAt
        };
}
