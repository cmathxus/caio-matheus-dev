using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;
using CaioMatheusDev.Api.Domain.BackendRoom;

namespace CaioMatheusDev.Api.Application.Services;

public sealed class BackendRoomService(
    IJwtTokenService jwtTokenService,
    IAuthUserStore authUserStore,
    IBackendRoomStore backendRoomStore) : IBackendRoomService
{
    private const int NoteMaxLength = 1_200;
    private const int DrawingNameMaxLength = 80;
    private const int DrawingDataUrlMaxLength = 1_500_000;

    public async Task<Result<BackendRoomSnapshot>> GetRoomAsync(
        string authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetAuthorizedUserAsync(authorizationHeader, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<BackendRoomSnapshot>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var user = userResult.Value!;
        var notes = await backendRoomStore.GetNotesAsync(user.Id, cancellationToken);
        var drawing = await backendRoomStore.GetDrawingAsync(user.Id, cancellationToken);
        var snapshot = new BackendRoomSnapshot(
            new AuthUserProfile(user.Id, user.Name, user.Email, user.CreatedAt),
            notes,
            drawing,
            DateTimeOffset.UtcNow);

        return Result<BackendRoomSnapshot>.Ok(snapshot);
    }

    public async Task<Result<BackendRoomNote>> CreateNoteAsync(
        string authorizationHeader,
        CreateBackendRoomNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetAuthorizedUserAsync(authorizationHeader, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<BackendRoomNote>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var contentResult = NormalizeNoteContent(request.Content);

        if (!contentResult.IsSuccess)
        {
            return Result<BackendRoomNote>.Fail(contentResult.Error!.Code, contentResult.Error.Message);
        }

        var now = DateTimeOffset.UtcNow;
        var note = new BackendRoomNote(
            Guid.NewGuid(),
            userResult.Value!.Id,
            contentResult.Value!,
            now,
            now);

        return Result<BackendRoomNote>.Ok(await backendRoomStore.AddNoteAsync(note, cancellationToken));
    }

    public async Task<Result<BackendRoomNote>> UpdateNoteAsync(
        string authorizationHeader,
        Guid noteId,
        UpdateBackendRoomNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetAuthorizedUserAsync(authorizationHeader, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<BackendRoomNote>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var contentResult = NormalizeNoteContent(request.Content);

        if (!contentResult.IsSuccess)
        {
            return Result<BackendRoomNote>.Fail(contentResult.Error!.Code, contentResult.Error.Message);
        }

        var note = await backendRoomStore.UpdateNoteAsync(
            userResult.Value!.Id,
            noteId,
            contentResult.Value!,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return note is null
            ? Result<BackendRoomNote>.Fail("note_not_found", "Note not found in this Backend Room.")
            : Result<BackendRoomNote>.Ok(note);
    }

    public async Task<Result<BackendRoomActionResult>> DeleteNoteAsync(
        string authorizationHeader,
        Guid noteId,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetAuthorizedUserAsync(authorizationHeader, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<BackendRoomActionResult>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var deleted = await backendRoomStore.DeleteNoteAsync(userResult.Value!.Id, noteId, cancellationToken);

        return deleted
            ? Result<BackendRoomActionResult>.Ok(new BackendRoomActionResult("Note deleted."))
            : Result<BackendRoomActionResult>.Fail("note_not_found", "Note not found in this Backend Room.");
    }

    public async Task<Result<BackendRoomDrawing>> SaveDrawingAsync(
        string authorizationHeader,
        SaveBackendRoomDrawingRequest request,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetAuthorizedUserAsync(authorizationHeader, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<BackendRoomDrawing>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var normalizedName = string.IsNullOrWhiteSpace(request.Name)
            ? "backend-room-sketch"
            : request.Name.Trim();

        if (normalizedName.Length > DrawingNameMaxLength)
        {
            return Result<BackendRoomDrawing>.Fail("invalid_drawing_name", $"Drawing name must have at most {DrawingNameMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(request.DataUrl) ||
            !request.DataUrl.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase))
        {
            return Result<BackendRoomDrawing>.Fail("invalid_drawing", "Send the canvas as a PNG data URL.");
        }

        if (request.DataUrl.Length > DrawingDataUrlMaxLength)
        {
            return Result<BackendRoomDrawing>.Fail("drawing_too_large", "Drawing is too large for this demo.");
        }

        var now = DateTimeOffset.UtcNow;
        var existing = await backendRoomStore.GetDrawingAsync(userResult.Value!.Id, cancellationToken);
        var drawing = new BackendRoomDrawing(
            existing?.Id ?? Guid.NewGuid(),
            userResult.Value.Id,
            normalizedName,
            request.DataUrl,
            existing?.CreatedAt ?? now,
            now);

        return Result<BackendRoomDrawing>.Ok(await backendRoomStore.SaveDrawingAsync(drawing, cancellationToken));
    }

    private async Task<Result<AuthUser>> GetAuthorizedUserAsync(
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var tokenResult = jwtTokenService.Validate(authorizationHeader);

        if (!tokenResult.IsSuccess)
        {
            return Result<AuthUser>.Fail(tokenResult.Error!.Code, tokenResult.Error.Message);
        }

        var payload = tokenResult.Value!;
        var user = await authUserStore.FindByIdAsync(payload.UserId, cancellationToken);

        return user is null
            ? Result<AuthUser>.Fail("user_not_found", "The token is valid, but the user no longer exists.")
            : Result<AuthUser>.Ok(user);
    }

    private static Result<string> NormalizeNoteContent(string content)
    {
        var normalized = content.Trim();

        if (normalized.Length < 2)
        {
            return Result<string>.Fail("empty_note", "Note must have at least 2 characters.");
        }

        if (normalized.Length > NoteMaxLength)
        {
            return Result<string>.Fail("note_too_long", $"Note must have at most {NoteMaxLength} characters.");
        }

        return Result<string>.Ok(normalized);
    }
}
