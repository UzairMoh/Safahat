using Safahat.Application.DTOs.Requests.Categories;
using Safahat.Application.DTOs.Responses.Categories;

namespace Safahat.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse> GetByIdAsync(Guid id);
    Task<CategoryResponse> GetBySlugAsync(string slug);
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(Guid categoryId, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(Guid categoryId);
        
    // Specialized operations
    Task<IEnumerable<CategoryResponse>> GetCategoriesWithPostCountAsync();
}