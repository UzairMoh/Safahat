using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Categories;
using Safahat.Application.DTOs.Responses.Categories;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

public class CategoriesController(ICategoryService categoryService) : BaseController
{
    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAllCategories()
    {
        var categories = await categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get categories with post count
    /// </summary>
    [HttpGet("with-post-count")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategoriesWithPostCount()
    {
        var categories = await categoryService.GetCategoriesWithPostCountAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(Guid id)
    {
        try
        {
            var category = await categoryService.GetByIdAsync(id);
            return Ok(category);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(CategoryResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CategoryResponse>> GetCategoryBySlug(string slug)
    {
        try
        {
            var category = await categoryService.GetBySlugAsync(slug);
            return Ok(category);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Create a new category - Admin only
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(CategoryResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = await categoryService.CreateAsync(request);
            return Created($"/api/categories/{category.Id}", category);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update a category - Admin only
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(CategoryResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var updatedCategory = await categoryService.UpdateAsync(id, request);
            return Ok(updatedCategory);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Delete a category - Admin only
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var result = await categoryService.DeleteAsync(id);
            return Ok(new { deleted = result });
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}