using System.Text.RegularExpressions;
using AutoMapper;
using Safahat.Application.DTOs.Requests.Categories;
using Safahat.Application.DTOs.Responses.Categories;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Application.Services;

public class CategoryService(
    ICategoryRepository categoryRepository,
    IMapper mapper) : ICategoryService
{
    public async Task<CategoryResponse> GetByIdAsync(Guid id)
    {
        var category = await categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new ApplicationException("Category not found");
        }

        return mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> GetBySlugAsync(string slug)
    {
        var category = await categoryRepository.GetBySlugAsync(slug);
        if (category == null)
        {
            throw new ApplicationException("Category not found");
        }

        return mapper.Map<CategoryResponse>(category);
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return mapper.Map<IEnumerable<CategoryResponse>>(categories);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        // Generate slug if not provided
        if (string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Name);
        }
        else
        {
            request.Slug = GenerateSlug(request.Slug);
        }

        // Check if slug is unique
        var isSlugUnique = await categoryRepository.IsSlugUniqueAsync(request.Slug);
        if (!isSlugUnique)
        {
            throw new ApplicationException("A category with this slug already exists");
        }

        var category = mapper.Map<Category>(request);
        var createdCategory = await categoryRepository.AddAsync(category);
        
        return mapper.Map<CategoryResponse>(createdCategory);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid categoryId, UpdateCategoryRequest request)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new ApplicationException("Category not found");
        }

        // Generate slug if name changed
        if (!string.IsNullOrEmpty(request.Name) && request.Name != category.Name && string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Name);
        }
        else if (!string.IsNullOrEmpty(request.Slug))
        {
            request.Slug = GenerateSlug(request.Slug);
        }

        // Check if slug is unique (if changed)
        if (!string.IsNullOrEmpty(request.Slug) && request.Slug != category.Slug)
        {
            var isSlugUnique = await categoryRepository.IsSlugUniqueAsync(request.Slug);
            if (!isSlugUnique)
            {
                throw new ApplicationException("A category with this slug already exists");
            }
        }

        // Update category properties
        mapper.Map(request, category);
        category.UpdatedAt = DateTime.UtcNow;

        await categoryRepository.UpdateAsync(category);
        return mapper.Map<CategoryResponse>(category);
    }

    public async Task<bool> DeleteAsync(Guid categoryId)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new ApplicationException("Category not found");
        }

        // Check if category is used in any posts before deletion
        // This might require a method in the post repository to check this

        await categoryRepository.DeleteAsync(categoryId);
        return true;
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesWithPostCountAsync()
    {
        // Get all categories
        var categories = await categoryRepository.GetAllAsync();
        
        // Map to response DTOs (the PostCount property should be populated by mapping profile)
        return mapper.Map<IEnumerable<CategoryResponse>>(categories);
    }

    #region Helper Methods

    private string GenerateSlug(string text)
    {
        // Convert to lowercase
        string slug = text.ToLowerInvariant();
        
        // Remove diacritics (accents)
        slug = RemoveDiacritics(slug);
        
        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s", "-");
        
        // Remove invalid characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        
        // Remove multiple hyphens
        slug = Regex.Replace(slug, @"-+", "-");
        
        // Trim hyphens from start and end
        slug = slug.Trim('-');
        
        return slug;
    }

    private string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    #endregion
}