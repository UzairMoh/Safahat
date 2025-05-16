using Safahat.Application.DTOs.Requests.Tags;
using Safahat.Application.DTOs.Responses.Tags;

namespace Safahat.Application.Interfaces;

public interface ITagService
{
    // Basic CRUD operations
    Task<TagResponse> GetByIdAsync(int id);
    Task<TagResponse> GetBySlugAsync(string slug);
    Task<IEnumerable<TagResponse>> GetAllAsync();
    Task<TagResponse> CreateAsync(CreateTagRequest request);
    Task<TagResponse> UpdateAsync(int tagId, UpdateTagRequest request);
    Task<bool> DeleteAsync(int tagId);
        
    // Specialized operations
    Task<IEnumerable<TagResponse>> GetTagsWithPostCountAsync();
    Task<IEnumerable<TagResponse>> GetPopularTagsAsync(int count);
}