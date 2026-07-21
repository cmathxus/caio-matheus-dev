using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.BackendRoom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaioMatheusDev.Api.Presentation.Controllers;

[Authorize]
[Route("api/backend-room")]
public sealed class BackendRoomController(IBackendRoomService backendRoomService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<BackendRoomSnapshot>>> GetRoom(
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<BackendRoomSnapshot>();
        }

        var result = await backendRoomService.GetRoomAsync(userId.Value, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPost("notes")]
    public async Task<ActionResult<ApiResponse<BackendRoomNote>>> CreateNote(
        CreateBackendRoomNoteRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<BackendRoomNote>();
        }

        var result = await backendRoomService.CreateNoteAsync(userId.Value, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPut("notes/{noteId:guid}")]
    public async Task<ActionResult<ApiResponse<BackendRoomNote>>> UpdateNote(
        Guid noteId,
        UpdateBackendRoomNoteRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<BackendRoomNote>();
        }

        var result = await backendRoomService.UpdateNoteAsync(userId.Value, noteId, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpDelete("notes/{noteId:guid}")]
    public async Task<ActionResult<ApiResponse<BackendRoomActionResult>>> DeleteNote(
        Guid noteId,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<BackendRoomActionResult>();
        }

        var result = await backendRoomService.DeleteNoteAsync(userId.Value, noteId, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPut("drawing")]
    public async Task<ActionResult<ApiResponse<BackendRoomDrawing>>> SaveDrawing(
        SaveBackendRoomDrawingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<BackendRoomDrawing>();
        }

        var result = await backendRoomService.SaveDrawingAsync(userId.Value, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpGet("community")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BackendRoomCommunityPost>>>> GetCommunityPosts(
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<IReadOnlyCollection<BackendRoomCommunityPost>>();
        }

        var result = await backendRoomService.GetCommunityPostsAsync(userId.Value, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPost("community")]
    public async Task<ActionResult<ApiResponse<BackendRoomCommunityPost>>> ShareDrawing(
        ShareBackendRoomDrawingRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<BackendRoomCommunityPost>();
        }

        var result = await backendRoomService.ShareDrawingAsync(userId.Value, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPost("community/{postId:guid}/like")]
    public async Task<ActionResult<ApiResponse<BackendRoomLikeResult>>> ToggleCommunityPostLike(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<BackendRoomLikeResult>();
        }

        var result = await backendRoomService.ToggleCommunityPostLikeAsync(userId.Value, postId, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpDelete("community/{postId:guid}")]
    public async Task<ActionResult<ApiResponse<BackendRoomActionResult>>> DeleteCommunityPost(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<BackendRoomActionResult>();
        }

        var result = await backendRoomService.DeleteCommunityPostAsync(userId.Value, postId, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    private static int ResolveFailureStatus(ApiError? error) =>
        error?.Code switch
        {
            "user_not_found" => StatusCodes.Status401Unauthorized,
            "note_not_found" or "community_post_not_found" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };
}
