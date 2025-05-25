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
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetAllComments()
    {
        var comments = await commentService.GetAllAsync();
        return Ok(comments);
    }

    /// <summary>
    /// Get comment by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CommentResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CommentResponse>> GetCommentById(Guid id)
    {
        try
        {
            var comment = await commentService.GetByIdAsync(id);
            return Ok(comment);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get comments by post ID
    /// </summary>
    [HttpGet("post/{postId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), 200)]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetCommentsByPost(Guid postId)
    {
        var comments = await commentService.GetCommentsByPostAsync(postId);
        return Ok(comments);
    }

    /// <summary>
    /// Get comments by user ID - User can only access their own comments, Admin can access any
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetCommentsByUser(Guid userId)
    {
        // Check if user can access this resource
        if (!UserCanAccessResource(userId))
        {
            return Forbid();
        }

        var comments = await commentService.GetCommentsByUserAsync(userId);
        return Ok(comments);
    }

    /// <summary>
    /// Get pending comments for moderation - Admin only
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetPendingComments()
    {
        var comments = await commentService.GetPendingCommentsAsync();
        return Ok(comments);
    }

    /// <summary>
    /// Create a comment - Authenticated users only
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(CommentResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CommentResponse>> CreateComment([FromBody] CreateCommentRequest request)
    {
        try
        {
            var comment = await commentService.CreateAsync(UserId, request);
            return Created($"/api/comments/{comment.Id}", comment);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Reply to a comment - Authenticated users only
    /// </summary>
    [HttpPost("{parentCommentId:guid}/reply")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(CommentResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CommentResponse>> ReplyToComment(Guid parentCommentId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var reply = await commentService.ReplyToCommentAsync(parentCommentId, UserId, request);
            return Created($"/api/comments/{reply.Id}", reply);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a comment - Author only
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(CommentResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CommentResponse>> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var updatedComment = await commentService.UpdateAsync(id, UserId, request);
            return Ok(updatedComment);
        }
        catch (ApplicationException ex)
        {
            return ex.Message.Contains("not found") 
                ? NotFound(ex.Message) 
                : Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Delete a comment - Author, Post Author, or Admin only
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteComment(Guid id)
    {
        try
        {
            var result = await commentService.DeleteAsync(id, UserId);
            return Ok(new { deleted = result });
        }
        catch (ApplicationException ex)
        {
            return ex.Message.Contains("not found") 
                ? NotFound(ex.Message) 
                : Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Approve a comment - Admin only
    /// </summary>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> ApproveComment(Guid id)
    {
        try
        {
            var result = await commentService.ApproveCommentAsync(id);
            return Ok(new { approved = result });
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Reject a comment - Admin only
    /// </summary>
    [HttpPut("{id:guid}/reject")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> RejectComment(Guid id)
    {
        try
        {
            var result = await commentService.RejectCommentAsync(id);
            return Ok(new { rejected = result });
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}