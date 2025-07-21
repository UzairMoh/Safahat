using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Responses.Comments;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

[Produces("application/json")]
public class CommentsController(ICommentService commentService) : BaseController
{
    /// <summary>
    /// Retrieves all comments (Admin only)
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
    /// Retrieves a specific comment by ID
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
    /// Retrieves all comments for a specific post
    /// </summary>
    [HttpGet("post/{postId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), 200)]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetCommentsByPost(Guid postId)
    {
        var comments = await commentService.GetCommentsByPostAsync(postId);
        return Ok(comments);
    }

    /// <summary>
    /// Retrieves comments by user
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetCommentsByUser(Guid userId)
    {
        if (!UserCanAccessResource(userId))
        {
            return Forbid();
        }

        var comments = await commentService.GetCommentsByUserAsync(userId);
        return Ok(comments);
    }

    /// <summary>
    /// Creates a new comment
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
    /// Creates a reply to an existing comment
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
    /// Updates an existing comment
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
            if (ex.Message.Contains("not found"))
                return NotFound(new { message = ex.Message });
            
            if (ex.Message.Contains("not authorized"))
                return Forbid();
                
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a comment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteComment(Guid id)
    {
        try
        {
            await commentService.DeleteAsync(id, UserId, IsAdmin);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(new { message = ex.Message });
            
            if (ex.Message.Contains("not authorized"))
                return Forbid();
                
            return BadRequest(new { message = ex.Message });
        }
    }
}