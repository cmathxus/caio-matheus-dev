using System.Collections.Concurrent;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.BackendRoom;

namespace CaioMatheusDev.Api.Infrastructure.Auth;

public sealed class InMemoryBackendRoomStore : IBackendRoomStore
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, BackendRoomNote>> notesByUserId = new();
    private readonly ConcurrentDictionary<Guid, BackendRoomDrawing> drawingsByUserId = new();
    private readonly ConcurrentDictionary<Guid, BackendRoomCommunityPost> communityPosts = new();
    private readonly ConcurrentDictionary<CommunityLikeKey, DateTimeOffset> communityLikes = new();

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

    public Task<IReadOnlyCollection<BackendRoomCommunityPost>> GetCommunityPostsAsync(
        Guid currentUserId,
        int take,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        DeleteExpiredCommunityPosts(now);

        return Task.FromResult<IReadOnlyCollection<BackendRoomCommunityPost>>(
            communityPosts.Values
                .Where(post => post.ExpiresAt > now)
                .OrderByDescending(post => post.CreatedAt)
                .Take(take)
                .Select(post => HydrateCommunityPost(post, currentUserId))
                .ToList());
    }

    public Task<BackendRoomCommunityPost> AddCommunityPostAsync(
        BackendRoomCommunityPost post,
        CancellationToken cancellationToken = default)
    {
        communityPosts[post.Id] = post;

        return Task.FromResult(post);
    }

    public Task<BackendRoomLikeResult?> ToggleCommunityPostLikeAsync(
        Guid currentUserId,
        Guid postId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        DeleteExpiredCommunityPosts(now);

        if (!communityPosts.TryGetValue(postId, out var post) || post.ExpiresAt <= now)
        {
            return Task.FromResult<BackendRoomLikeResult?>(null);
        }

        var key = new CommunityLikeKey(postId, currentUserId);
        var liked = !communityLikes.TryRemove(key, out _);

        if (liked)
        {
            communityLikes[key] = now;
        }

        var likesCount = communityLikes.Keys.Count(like => like.PostId == postId);

        return Task.FromResult<BackendRoomLikeResult?>(new BackendRoomLikeResult(postId, likesCount, liked));
    }

    public Task<bool> DeleteCommunityPostAsync(
        Guid currentUserId,
        Guid postId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        DeleteExpiredCommunityPosts(now);

        if (!communityPosts.TryGetValue(postId, out var post) || post.UserId != currentUserId || post.ExpiresAt <= now)
        {
            return Task.FromResult(false);
        }

        var deleted = communityPosts.TryRemove(postId, out _);

        if (deleted)
        {
            foreach (var key in communityLikes.Keys.Where(like => like.PostId == postId).ToList())
            {
                communityLikes.TryRemove(key, out _);
            }
        }

        return Task.FromResult(deleted);
    }

    private BackendRoomCommunityPost HydrateCommunityPost(
        BackendRoomCommunityPost post,
        Guid currentUserId)
    {
        var likesCount = communityLikes.Keys.Count(like => like.PostId == post.Id);
        var likedByCurrentUser = communityLikes.ContainsKey(new CommunityLikeKey(post.Id, currentUserId));

        return post with { LikesCount = likesCount, LikedByCurrentUser = likedByCurrentUser };
    }

    private void DeleteExpiredCommunityPosts(DateTimeOffset now)
    {
        var expiredPostIds = communityPosts.Values
            .Where(post => post.ExpiresAt <= now)
            .Select(post => post.Id)
            .ToHashSet();

        foreach (var postId in expiredPostIds)
        {
            communityPosts.TryRemove(postId, out _);
        }

        foreach (var key in communityLikes.Keys.Where(like => expiredPostIds.Contains(like.PostId)).ToList())
        {
            communityLikes.TryRemove(key, out _);
        }
    }

    private readonly record struct CommunityLikeKey(Guid PostId, Guid UserId);
}
