using Microsoft.EntityFrameworkCore;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Infrastructure.Repositories.Implementations;

public class PostRepository(SafahatDbContext context) : Repository<Post>(context), IPostRepository
{
    public override async Task<Post?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(p => p.Author)
            .Include(p => p.PostCategories)
            .ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
       
    public async Task<IEnumerable<Post>> GetPublishedPostsAsync()
    {
        return await DbSet
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByAuthorAsync(Guid authorId)
    {
        return await DbSet
            .Include(p => p.Author)
            .Include(p => p.PostCategories)
            .ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags)
            .ThenInclude(pt => pt.Tag)
            .Where(p => p.AuthorId == authorId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostBySlugAsync(string slug)
    {
        return await DbSet
            .Include(p => p.Author)
            .Include(p => p.Comments)
            .Include(p => p.PostCategories)
            .ThenInclude(pc => pc.Category)
            .Include(p => p.PostTags)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PostStatus.Published);
    }

    public async Task<IEnumerable<Post>> GetFeaturedPostsAsync()
    {
        return await DbSet
            .Where(p => p.IsFeatured && p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> SearchPostsAsync(string searchTerm)
    {
        return await DbSet
            .Where(p => p.Status == PostStatus.Published &&
                       (p.Title.Contains(searchTerm) || 
                        p.Content.Contains(searchTerm) ||
                        p.Summary.Contains(searchTerm)))
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByCategoryAsync(Guid categoryId)
    {
        return await DbSet
            .Where(p => p.Status == PostStatus.Published)
            .Include(p => p.PostCategories)
            .Where(p => p.PostCategories.Any(pc => pc.CategoryId == categoryId))
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByTagAsync(Guid tagId)
    {
        return await DbSet
            .Where(p => p.Status == PostStatus.Published)
            .Include(p => p.PostTags)
            .Where(p => p.PostTags.Any(pt => pt.TagId == tagId))
            .OrderByDescending(p => p.PublishedAt)
            .ToListAsync();
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludePostId = null)
    {
        var query = DbSet.Where(p => p.Slug == slug);
    
        if (excludePostId.HasValue)
        {
            query = query.Where(p => p.Id != excludePostId.Value);
        }
    
        return await query.AnyAsync();
    }
}