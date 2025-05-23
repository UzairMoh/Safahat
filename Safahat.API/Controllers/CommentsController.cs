using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Responses.Comments;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

public class CommentsController(ICommentService commentService) : BaseController
{
    /// <summary>
    /// Get all comments - Admin only
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetAllComments()
    {
        var comments = await commentService.GetAllAsync();
        return Success(comments);
    }

    /// <summary>
    /// Get comment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommentResponse>> GetCommentById(Guid id)
    {
        try
        {
            var comment = await commentService.GetByIdAsync(id);
            return Success(comment);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Get comments by post ID
    /// </summary>
    [HttpGet("post/{postId:guid}")]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetCommentsByPost(Guid postId)
    {
        var comments = await commentService.GetCommentsByPostAsync(postId);
        return Success(comments);
    }

    /// <summary>
    /// Get comments by user ID - User can only access their own comments, Admin can access any
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetCommentsByUser(Guid userId)
    {
        // Check if user can access this resource
        if (!await UserCanAccessResourceAsync(userId))
        {
            return ForbidWithMessage();
        }

        var comments = await commentService.GetCommentsByUserAsync(userId);
        return Success(comments);
    }

    /// <summary>
    /// Get pending comments for moderation - Admin only
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetPendingComments()
    {
        var comments = await commentService.GetPendingCommentsAsync();
        return Success(comments);
    }

    /// <summary>
    /// Create a comment - Authenticated users only
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<CommentResponse>> CreateComment([FromBody] CreateCommentRequest request)
    {
        try
        {
            var comment = await commentService.CreateAsync(UserId, request);
            return CreatedWithMessage("Comment created successfully", comment);
        }
        catch (ApplicationException ex)
        {
            return BadRequestWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Reply to a comment - Authenticated users only
    /// </summary>
    [HttpPost("{parentCommentId:guid}/reply")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<CommentResponse>> ReplyToComment(Guid parentCommentId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var reply = await commentService.ReplyToCommentAsync(parentCommentId, UserId, request);
            return CreatedWithMessage("Reply created successfully", reply);
        }
        catch (ApplicationException ex)
        {
            return BadRequestWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Update a comment - Author only
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<CommentResponse>> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var updatedComment = await commentService.UpdateAsync(id, UserId, request);
            return Success(updatedComment);
        }
        catch (ApplicationException ex)
        {
            return ex.Message.Contains("not found") 
                ? NotFoundWithMessage(ex.Message) 
                : ForbidWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Delete a comment - Author, Post Author, or Admin only
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult> DeleteComment(Guid id)
    {
        try
        {
            var result = await commentService.DeleteAsync(id, UserId);
            return Success(new { deleted = result });
        }
        catch (ApplicationException ex)
        {
            return ex.Message.Contains("not found") 
                ? NotFoundWithMessage(ex.Message) 
                : ForbidWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Approve a comment - Admin only
    /// </summary>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> ApproveComment(Guid id)
    {
        try
        {
            var result = await commentService.ApproveCommentAsync(id);
            return Success(new { approved = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Reject a comment - Admin only
    /// </summary>
    [HttpPut("{id:guid}/reject")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> RejectComment(Guid id)
    {
        try
        {
            var result = await commentService.RejectCommentAsync(id);
            return Success(new { rejected = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
}