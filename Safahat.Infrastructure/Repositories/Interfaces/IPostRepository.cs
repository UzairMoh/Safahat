using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetPublishedPostsAsync();
    Task<IEnumerable<Post>> GetPostsByAuthorAsync(int authorId);
    Task<Post> GetPostBySlugAsync(string slug);
    Task<IEnumerable<Post>> GetFeaturedPostsAsync();
    Task<IEnumerable<Post>> SearchPostsAsync(string searchTerm);
    Task<IEnumerable<Post>> GetPostsByCategoryAsync(int categoryId);
    Task<IEnumerable<Post>> GetPostsByTagAsync(int tagId);
}