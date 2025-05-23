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
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAllCategories()
    {
        var categories = await categoryService.GetAllAsync();
        return Success(categories);
    }

    /// <summary>
    /// Get categories with post count
    /// </summary>
    [HttpGet("with-post-count")]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategoriesWithPostCount()
    {
        var categories = await categoryService.GetCategoriesWithPostCountAsync();
        return Success(categories);
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryById(Guid id)
    {
        try
        {
            var category = await categoryService.GetByIdAsync(id);
            return Success(category);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Get category by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CategoryResponse>> GetCategoryBySlug(string slug)
    {
        try
        {
            var category = await categoryService.GetBySlugAsync(slug);
            return Success(category);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Create a new category - Admin only
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = await categoryService.CreateAsync(request);
            return CreatedWithMessage("Category created successfully", category);
        }
        catch (ApplicationException ex)
        {
            return BadRequestWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Update a category - Admin only
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var updatedCategory = await categoryService.UpdateAsync(id, request);
            return Success(updatedCategory);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Delete a category - Admin only
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var result = await categoryService.DeleteAsync(id);
            return Success(new { deleted = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
}