using Microsoft.AspNetCore.Http;
using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Posts;

namespace Safahat.Application.Interfaces;

public interface IPostService
{
    Task<PostResponse> GetByIdAsync(Guid id);
    Task<PostResponse> GetBySlugAsync(string slug, ISession session);
    Task<IEnumerable<PostResponse>> GetAllAsync();
    Task<PostResponse> CreateAsync(Guid authorId, CreatePostRequest request);
    Task<PostResponse> UpdateAsync(Guid postId, UpdatePostRequest request);
    Task<bool> DeleteAsync(Guid postId);
    Task<IEnumerable<PostResponse>> GetPublishedPostsAsync(int pageNumber, int pageSize);
    Task<IEnumerable<PostResponse>> GetPostsByAuthorAsync(Guid authorId, int pageNumber, int pageSize);
    Task<IEnumerable<PostResponse>> GetFeaturedPostsAsync();
    Task<IEnumerable<PostResponse>> SearchPostsAsync(string searchTerm, int pageNumber, int pageSize);
    Task<IEnumerable<PostResponse>> GetPostsByCategoryAsync(Guid categoryId, int pageNumber, int pageSize);
    Task<IEnumerable<PostResponse>> GetPostsByTagAsync(Guid tagId, int pageNumber, int pageSize);
    Task<bool> PublishPostAsync(Guid postId);
    Task<bool> UnpublishPostAsync(Guid postId);
    Task<bool> FeaturePostAsync(Guid postId);
    Task<bool> UnfeaturePostAsync(Guid postId);
}