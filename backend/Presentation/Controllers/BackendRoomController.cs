using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.BackendRoom;
using Microsoft.AspNetCore.Mvc;

namespace CaioMatheusDev.Api.Presentation.Controllers;

[Route("api/backend-room")]
public sealed class BackendRoomController(IBackendRoomService backendRoomService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<BackendRoomSnapshot>>> GetRoom(
        [FromHeader(Name = "Authorization")] string? authorization,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.GetRoomAsync(authorization ?? string.Empty, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPost("notes")]
    public async Task<ActionResult<ApiResponse<BackendRoomNote>>> CreateNote(
        [FromHeader(Name = "Authorization")] string? authorization,
        CreateBackendRoomNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.CreateNoteAsync(authorization ?? string.Empty, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPut("notes/{noteId:guid}")]
    public async Task<ActionResult<ApiResponse<BackendRoomNote>>> UpdateNote(
        [FromHeader(Name = "Authorization")] string? authorization,
        Guid noteId,
        UpdateBackendRoomNoteRequest request,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.UpdateNoteAsync(authorization ?? string.Empty, noteId, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpDelete("notes/{noteId:guid}")]
    public async Task<ActionResult<ApiResponse<BackendRoomActionResult>>> DeleteNote(
        [FromHeader(Name = "Authorization")] string? authorization,
        Guid noteId,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.DeleteNoteAsync(authorization ?? string.Empty, noteId, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPut("drawing")]
    public async Task<ActionResult<ApiResponse<BackendRoomDrawing>>> SaveDrawing(
        [FromHeader(Name = "Authorization")] string? authorization,
        SaveBackendRoomDrawingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.SaveDrawingAsync(authorization ?? string.Empty, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpGet("community")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BackendRoomCommunityPost>>>> GetCommunityPosts(
        [FromHeader(Name = "Authorization")] string? authorization,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.GetCommunityPostsAsync(authorization ?? string.Empty, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPost("community")]
    public async Task<ActionResult<ApiResponse<BackendRoomCommunityPost>>> ShareDrawing(
        [FromHeader(Name = "Authorization")] string? authorization,
        ShareBackendRoomDrawingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.ShareDrawingAsync(authorization ?? string.Empty, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPost("community/{postId:guid}/like")]
    public async Task<ActionResult<ApiResponse<BackendRoomLikeResult>>> ToggleCommunityPostLike(
        [FromHeader(Name = "Authorization")] string? authorization,
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.ToggleCommunityPostLikeAsync(authorization ?? string.Empty, postId, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpDelete("community/{postId:guid}")]
    public async Task<ActionResult<ApiResponse<BackendRoomActionResult>>> DeleteCommunityPost(
        [FromHeader(Name = "Authorization")] string? authorization,
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await backendRoomService.DeleteCommunityPostAsync(authorization ?? string.Empty, postId, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    private static int ResolveFailureStatus(ApiError? error) =>
        error?.Code switch
        {
            "missing_token" or
                "invalid_token" or
                "invalid_signature" or
                "expired_token" or
                "invalid_payload" or
                "invalid_token_context" or
                "user_not_found" => StatusCodes.Status401Unauthorized,
            "note_not_found" or "community_post_not_found" => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };
}
