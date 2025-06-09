using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Posts;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Application.Services;

public class PostService(
    IPostRepository postRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository,
    IMapper mapper)
    : IPostService
{
    public async Task<PostResponse> GetByIdAsync(Guid id)
    {
        var post = await postRepository.GetByIdAsync(id);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        return mapper.Map<PostResponse>(post);
    }

    public async Task<PostResponse> GetBySlugAsync(string slug, ISession session)
    {
        var post = await postRepository.GetPostBySlugAsync(slug);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        var sessionKey = $"last_viewed_{post.Id}";
        var lastViewedStr = session.GetString(sessionKey);

        if (DateTime.TryParse(lastViewedStr, out var lastViewed))
        {
            if (DateTime.UtcNow.Subtract(lastViewed).TotalMinutes > 30)
            {
                post.ViewCount++;
                await postRepository.UpdateAsync(post);
                session.SetString(sessionKey, DateTime.UtcNow.ToString());
            }
        }
        else
        {
            post.ViewCount++;
            await postRepository.UpdateAsync(post);
            session.SetString(sessionKey, DateTime.UtcNow.ToString());
        }

        return mapper.Map<PostResponse>(post);
    }

    public async Task<IEnumerable<PostResponse>> GetAllAsync()
    {
        var posts = await postRepository.GetAllAsync();
        return mapper.Map<IEnumerable<PostResponse>>(posts);
    }

    public async Task<PostResponse> CreateAsync(Guid authorId, CreatePostRequest request)
    {
        var post = mapper.Map<Post>(request);
        post.AuthorId = authorId;
        
        post.Slug = await GenerateUniqueSlugAsync(request.Title);

        if (!request.IsDraft)
        {
            post.Status = PostStatus.Published;
            post.PublishedAt = DateTime.UtcNow;
        }
        else
        {
            post.Status = PostStatus.Draft;
        }

        var createdPost = await postRepository.AddAsync(post);

        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            createdPost.PostCategories = new List<PostCategory>();
            foreach (var categoryId in request.CategoryIds)
            {
                var category = await categoryRepository.GetByIdAsync(categoryId);
                if (category != null)
                {
                    createdPost.PostCategories.Add(new PostCategory
                    {
                        PostId = createdPost.Id,
                        CategoryId = categoryId
                    });
                }
            }
        }

        if (request.Tags != null && request.Tags.Any())
        {
            createdPost.PostTags = new List<PostTag>();
            foreach (var tagName in request.Tags)
            {
                var normalizedName = tagName.Trim().ToLower();
                var slug = GenerateSlug(normalizedName);
                
                var tag = await tagRepository.GetBySlugAsync(slug);
                if (tag == null)
                {
                    tag = new Tag
                    {
                        Name = normalizedName,
                        Slug = slug
                    };
                    tag = await tagRepository.AddAsync(tag);
                }
                
                createdPost.PostTags.Add(new PostTag
                {
                    PostId = createdPost.Id,
                    TagId = tag.Id
                });
            }
        }

        await postRepository.UpdateAsync(createdPost);

        var completePost = await postRepository.GetByIdAsync(createdPost.Id);
        return mapper.Map<PostResponse>(completePost);
    }

    public async Task<PostResponse> UpdateAsync(Guid postId, UpdatePostRequest request)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        mapper.Map(request, post);
        post.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.Title) && request.Title != post.Title)
        {
            post.Slug = await GenerateUniqueSlugAsync(request.Title, post.Id);
        }

        if (request.CategoryIds != null)
        {
            if (post.PostCategories != null)
            {
                post.PostCategories.Clear();
            }
            else
            {
                post.PostCategories = new List<PostCategory>();
            }

            foreach (var categoryId in request.CategoryIds)
            {
                var category = await categoryRepository.GetByIdAsync(categoryId);
                if (category != null)
                {
                    post.PostCategories.Add(new PostCategory
                    {
                        PostId = post.Id,
                        CategoryId = categoryId
                    });
                }
            }
        }

        if (request.Tags != null)
        {
            if (post.PostTags != null)
            {
                post.PostTags.Clear();
            }
            else
            {
                post.PostTags = new List<PostTag>();
            }

            foreach (var tagName in request.Tags)
            {
                var normalizedName = tagName.Trim().ToLower();
                var slug = GenerateSlug(normalizedName);
                
                var tag = await tagRepository.GetBySlugAsync(slug);
                if (tag == null)
                {
                    tag = new Tag
                    {
                        Name = normalizedName,
                        Slug = slug
                    };
                    tag = await tagRepository.AddAsync(tag);
                }
                
                post.PostTags.Add(new PostTag
                {
                    PostId = post.Id,
                    TagId = tag.Id
                });
            }
        }

        await postRepository.UpdateAsync(post);

        var updatedPost = await postRepository.GetByIdAsync(post.Id);
        return mapper.Map<PostResponse>(updatedPost);
    }

    public async Task<bool> DeleteAsync(Guid postId)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        await postRepository.DeleteAsync(postId);
        return true;
    }

    public async Task<IEnumerable<PostResponse>> GetPublishedPostsAsync(int pageNumber, int pageSize)
    {
        var posts = await postRepository.GetPublishedPostsAsync();
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<IEnumerable<PostResponse>> GetPostsByAuthorAsync(Guid authorId, int pageNumber, int pageSize)
    {
        var posts = await postRepository.GetPostsByAuthorAsync(authorId);
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<IEnumerable<PostResponse>> GetFeaturedPostsAsync()
    {
        var posts = await postRepository.GetFeaturedPostsAsync();
        return mapper.Map<IEnumerable<PostResponse>>(posts);
    }

    public async Task<IEnumerable<PostResponse>> SearchPostsAsync(string searchTerm, int pageNumber, int pageSize)
    {
        var posts = await postRepository.SearchPostsAsync(searchTerm);
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<IEnumerable<PostResponse>> GetPostsByCategoryAsync(Guid categoryId, int pageNumber, int pageSize)
    {
        var posts = await postRepository.GetPostsByCategoryAsync(categoryId);
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<IEnumerable<PostResponse>> GetPostsByTagAsync(Guid tagId, int pageNumber, int pageSize)
    {
        var posts = await postRepository.GetPostsByTagAsync(tagId);
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<bool> PublishPostAsync(Guid postId)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

        await postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> UnpublishPostAsync(Guid postId)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        post.Status = PostStatus.Draft;
        post.UpdatedAt = DateTime.UtcNow;

        await postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> FeaturePostAsync(Guid postId)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        post.IsFeatured = true;
        post.UpdatedAt = DateTime.UtcNow;

        await postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> UnfeaturePostAsync(Guid postId)
    {
        var post = await postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        post.IsFeatured = false;
        post.UpdatedAt = DateTime.UtcNow;

        await postRepository.UpdateAsync(post);
        return true;
    }

    #region Helper Methods

    /// <summary>
    /// Generates a unique slug by checking database and appending numbers if needed
    /// </summary>
    private async Task<string> GenerateUniqueSlugAsync(string title, Guid? excludePostId = null)
    {
        var baseSlug = GenerateSlug(title);
        var slug = baseSlug;
        var counter = 1;

        // Keep checking until we find a unique slug
        while (await IsSlugExistsAsync(slug, excludePostId))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    /// <summary>
    /// Checks if a slug already exists in the database
    /// </summary>
    private async Task<bool> IsSlugExistsAsync(string slug, Guid? excludePostId = null)
    {
        var posts = await postRepository.GetAllAsync();
        
        if (excludePostId.HasValue)
        {
            // When updating, exclude the current post from the check
            return posts.Any(p => p.Slug == slug && p.Id != excludePostId.Value);
        }
        
        return posts.Any(p => p.Slug == slug);
    }

    /// <summary>
    /// Generates a SEO-friendly slug from a title (unchanged from original)
    /// </summary>
    private string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "untitled";

        string slug = title.ToLowerInvariant();
        slug = RemoveDiacritics(slug);
        slug = Regex.Replace(slug, @"\s", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        
        return string.IsNullOrEmpty(slug) ? "untitled" : slug;
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