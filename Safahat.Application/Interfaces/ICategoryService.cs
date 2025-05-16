using Safahat.Application.DTOs.Requests.Categories;
using Safahat.Application.DTOs.Responses.Categories;

namespace Safahat.Application.Interfaces;

public interface ICategoryService
{
    // Basic CRUD operations
    Task<CategoryResponse> GetByIdAsync(int id);
    Task<CategoryResponse> GetBySlugAsync(string slug);
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(int categoryId, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int categoryId);
        
    // Specialized operations
    Task<IEnumerable<CategoryResponse>> GetCategoriesWithPostCountAsync();
}