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
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetAllTags()
    {
        var tags = await tagService.GetAllAsync();
        return Success(tags);
    }

    /// <summary>
    /// Get tags with post count
    /// </summary>
    [HttpGet("with-post-count")]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetTagsWithPostCount()
    {
        var tags = await tagService.GetTagsWithPostCountAsync();
        return Success(tags);
    }

    /// <summary>
    /// Get popular tags
    /// </summary>
    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetPopularTags([FromQuery] int count = 10)
    {
        var tags = await tagService.GetPopularTagsAsync(count);
        return Success(tags);
    }

    /// <summary>
    /// Get tag by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TagResponse>> GetTagById(Guid id)
    {
        try
        {
            var tag = await tagService.GetByIdAsync(id);
            return Success(tag);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Get tag by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<TagResponse>> GetTagBySlug(string slug)
    {
        try
        {
            var tag = await tagService.GetBySlugAsync(slug);
            return Success(tag);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Create a new tag - Admin only
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<TagResponse>> CreateTag([FromBody] CreateTagRequest request)
    {
        try
        {
            var tag = await tagService.CreateAsync(request);
            return CreatedWithMessage("Tag created successfully", tag);
        }
        catch (ApplicationException ex)
        {
            return BadRequestWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Update a tag - Admin only
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<TagResponse>> UpdateTag(Guid id, [FromBody] UpdateTagRequest request)
    {
        try
        {
            var updatedTag = await tagService.UpdateAsync(id, request);
            return Success(updatedTag);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Delete a tag - Admin only
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteTag(Guid id)
    {
        try
        {
            var result = await tagService.DeleteAsync(id);
            return Success(new { deleted = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
}