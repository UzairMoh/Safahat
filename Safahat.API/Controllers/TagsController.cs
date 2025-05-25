using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Tags;
using Safahat.Application.DTOs.Responses.Tags;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

public class TagsController(ITagService tagService) : BaseController
{
    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TagResponse>), 200)]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetAllTags()
    {
        var tags = await tagService.GetAllAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Get tags with post count
    /// </summary>
    [HttpGet("with-post-count")]
    [ProducesResponseType(typeof(IEnumerable<TagResponse>), 200)]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetTagsWithPostCount()
    {
        var tags = await tagService.GetTagsWithPostCountAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Get popular tags
    /// </summary>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(IEnumerable<TagResponse>), 200)]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetPopularTags([FromQuery] int count = 10)
    {
        var tags = await tagService.GetPopularTagsAsync(count);
        return Ok(tags);
    }

    /// <summary>
    /// Get tag by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TagResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagResponse>> GetTagById(Guid id)
    {
        try
        {
            var tag = await tagService.GetByIdAsync(id);
            return Ok(tag);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get tag by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(TagResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagResponse>> GetTagBySlug(string slug)
    {
        try
        {
            var tag = await tagService.GetBySlugAsync(slug);
            return Ok(tag);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Create a new tag - Admin only
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(TagResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<TagResponse>> CreateTag([FromBody] CreateTagRequest request)
    {
        try
        {
            var tag = await tagService.CreateAsync(request);
            return Created($"/api/tags/{tag.Id}", tag);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a tag - Admin only
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(TagResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TagResponse>> UpdateTag(Guid id, [FromBody] UpdateTagRequest request)
    {
        try
        {
            var updatedTag = await tagService.UpdateAsync(id, request);
            return Ok(updatedTag);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete a tag - Admin only
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteTag(Guid id)
    {
        try
        {
            var result = await tagService.DeleteAsync(id);
            return Ok(new { deleted = result });
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}