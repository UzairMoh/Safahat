using Safahat.Application.DTOs.Requests.Tags;
using Safahat.Application.DTOs.Responses.Tags;

namespace Safahat.Application.Interfaces;

public interface ITagService
{
    Task<TagResponse> GetByIdAsync(Guid id);
    Task<TagResponse> GetBySlugAsync(string slug);
    Task<IEnumerable<TagResponse>> GetAllAsync();
    Task<TagResponse> CreateAsync(CreateTagRequest request);
    Task<TagResponse> UpdateAsync(Guid tagId, UpdateTagRequest request);
    Task<bool> DeleteAsync(Guid tagId);
    Task<IEnumerable<TagResponse>> GetTagsWithPostCountAsync();
    Task<IEnumerable<TagResponse>> GetPopularTagsAsync(int count);
}