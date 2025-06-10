using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Categories;
using Safahat.Application.DTOs.Responses.Categories;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

[Produces("application/json")]
public class CategoriesController(ICategoryService categoryService) : BaseController
{
    /// <summary>
    /// Retrieves all categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAllCategories()
    {
        var categories = await categoryService.GetAllAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves categories with post counts
    /// </summary>
    [HttpGet("with-post-count")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), 200)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategoriesWithPostCount()
    {
        var categories = await categoryService.GetCategoriesWithPostCountAsync();
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a specific category by ID
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
    /// Retrieves a specific category by slug
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
    /// Creates a new category (Admin only)
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
    /// Updates an existing category (Admin only)
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
    /// Deletes a category (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        try
        {
            await categoryService.DeleteAsync(id);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}