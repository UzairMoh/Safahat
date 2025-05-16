using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Posts;

namespace Safahat.Application.Interfaces;

public interface IPostService
{
    // Basic CRUD operations
    Task<PostResponse> GetByIdAsync(int id);
    Task<PostResponse> GetBySlugAsync(string slug);
    Task<IEnumerable<PostResponse>> GetAllAsync();
    Task<PostResponse> CreateAsync(int authorId, CreatePostRequest request);
    Task<PostResponse> UpdateAsync(int postId, UpdatePostRequest request);
    Task<bool> DeleteAsync(int postId);
        
    // Specialized operations
    Task<IEnumerable<PostResponse>> GetPublishedPostsAsync(int pageNumber, int pageSize);
    Task<IEnumerable<PostResponse>> GetPostsByAuthorAsync(int authorId, int pageNumber, int pageSize);
    Task<IEnumerable<PostResponse>> GetFeaturedPostsAsync();
    Task<IEnumerable<PostResponse>> SearchPostsAsync(string searchTerm, int pageNumber, int pageSize);
    Task<IEnumerable<PostResponse>> GetPostsByCategoryAsync(int categoryId, int pageNumber, int pageSize);
    Task<IEnumerable<PostResponse>> GetPostsByTagAsync(int tagId, int pageNumber, int pageSize);
    Task<bool> PublishPostAsync(int postId);
    Task<bool> UnpublishPostAsync(int postId);
    Task<bool> FeaturePostAsync(int postId);
    Task<bool> UnfeaturePostAsync(int postId);
}