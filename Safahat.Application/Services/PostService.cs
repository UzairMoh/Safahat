using System.Text.RegularExpressions;
using AutoMapper;
using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Posts;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;

    public PostService(
        IPostRepository postRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        IMapper mapper)
    {
        _postRepository = postRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _mapper = mapper;
    }

    public async Task<PostResponse> GetByIdAsync(int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        return _mapper.Map<PostResponse>(post);
    }

    public async Task<PostResponse> GetBySlugAsync(string slug)
    {
        var post = await _postRepository.GetPostBySlugAsync(slug);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        // Increment view count
        post.ViewCount++;
        await _postRepository.UpdateAsync(post);

        return _mapper.Map<PostResponse>(post);
    }

    public async Task<IEnumerable<PostResponse>> GetAllAsync()
    {
        var posts = await _postRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PostResponse>>(posts);
    }

    public async Task<PostResponse> CreateAsync(int authorId, CreatePostRequest request)
    {
        var post = _mapper.Map<Post>(request);
        
        // Set the author ID
        post.AuthorId = authorId;
        
        // Generate a slug from the title
        post.Slug = GenerateSlug(request.Title);
        
        // Set publication status
        if (!request.IsDraft)
        {
            post.Status = PostStatus.Published;
            post.PublishedAt = DateTime.UtcNow;
        }
        else
        {
            post.Status = PostStatus.Draft;
        }

        // Save post to get ID
        var createdPost = await _postRepository.AddAsync(post);

        // Handle categories
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            createdPost.PostCategories = new List<PostCategory>();
            foreach (var categoryId in request.CategoryIds)
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId);
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

        // Handle tags
        if (request.Tags != null && request.Tags.Any())
        {
            createdPost.PostTags = new List<PostTag>();
            foreach (var tagName in request.Tags)
            {
                // Normalize tag name
                var normalizedName = tagName.Trim().ToLower();
                var slug = GenerateSlug(normalizedName);
                
                // Check if tag exists
                var tag = await _tagRepository.GetBySlugAsync(slug);
                if (tag == null)
                {
                    // Create new tag
                    tag = new Tag
                    {
                        Name = normalizedName,
                        Slug = slug
                    };
                    tag = await _tagRepository.AddAsync(tag);
                }
                
                // Add tag to post
                createdPost.PostTags.Add(new PostTag
                {
                    PostId = createdPost.Id,
                    TagId = tag.Id
                });
            }
        }

        // Update post with categories and tags
        await _postRepository.UpdateAsync(createdPost);

        // Get complete post with relationships
        var completePost = await _postRepository.GetByIdAsync(createdPost.Id);
        return _mapper.Map<PostResponse>(completePost);
    }

    public async Task<PostResponse> UpdateAsync(int postId, UpdatePostRequest request)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        // Update post properties
        _mapper.Map(request, post);
        post.UpdatedAt = DateTime.UtcNow;

        // Update slug if title changed
        if (!string.IsNullOrEmpty(request.Title) && request.Title != post.Title)
        {
            post.Slug = GenerateSlug(request.Title);
        }

        // Handle categories (remove existing and add new)
        if (request.CategoryIds != null)
        {
            // Clear existing categories
            if (post.PostCategories != null)
            {
                post.PostCategories.Clear();
            }
            else
            {
                post.PostCategories = new List<PostCategory>();
            }

            // Add new categories
            foreach (var categoryId in request.CategoryIds)
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId);
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

        // Handle tags (remove existing and add new)
        if (request.Tags != null)
        {
            // Clear existing tags
            if (post.PostTags != null)
            {
                post.PostTags.Clear();
            }
            else
            {
                post.PostTags = new List<PostTag>();
            }

            // Add new tags
            foreach (var tagName in request.Tags)
            {
                // Normalize tag name
                var normalizedName = tagName.Trim().ToLower();
                var slug = GenerateSlug(normalizedName);
                
                // Check if tag exists
                var tag = await _tagRepository.GetBySlugAsync(slug);
                if (tag == null)
                {
                    // Create new tag
                    tag = new Tag
                    {
                        Name = normalizedName,
                        Slug = slug
                    };
                    tag = await _tagRepository.AddAsync(tag);
                }
                
                // Add tag to post
                post.PostTags.Add(new PostTag
                {
                    PostId = post.Id,
                    TagId = tag.Id
                });
            }
        }

        // Update post in database
        await _postRepository.UpdateAsync(post);

        // Get complete post with relationships
        var updatedPost = await _postRepository.GetByIdAsync(post.Id);
        return _mapper.Map<PostResponse>(updatedPost);
    }

    public async Task<bool> DeleteAsync(int postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        await _postRepository.DeleteAsync(postId);
        return true;
    }

    public async Task<IEnumerable<PostResponse>> GetPublishedPostsAsync(int pageNumber, int pageSize)
    {
        var posts = await _postRepository.GetPublishedPostsAsync();
        
        // Apply pagination manually
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return _mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<IEnumerable<PostResponse>> GetPostsByAuthorAsync(int authorId, int pageNumber, int pageSize)
    {
        var posts = await _postRepository.GetPostsByAuthorAsync(authorId);
        
        // Apply pagination manually
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return _mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<IEnumerable<PostResponse>> GetFeaturedPostsAsync()
    {
        var posts = await _postRepository.GetFeaturedPostsAsync();
        return _mapper.Map<IEnumerable<PostResponse>>(posts);
    }

    public async Task<IEnumerable<PostResponse>> SearchPostsAsync(string searchTerm, int pageNumber, int pageSize)
    {
        var posts = await _postRepository.SearchPostsAsync(searchTerm);
        
        // Apply pagination manually
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return _mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<IEnumerable<PostResponse>> GetPostsByCategoryAsync(int categoryId, int pageNumber, int pageSize)
    {
        var posts = await _postRepository.GetPostsByCategoryAsync(categoryId);
        
        // Apply pagination manually
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return _mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<IEnumerable<PostResponse>> GetPostsByTagAsync(int tagId, int pageNumber, int pageSize)
    {
        var posts = await _postRepository.GetPostsByTagAsync(tagId);
        
        // Apply pagination manually
        var paginatedPosts = posts
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
        
        return _mapper.Map<IEnumerable<PostResponse>>(paginatedPosts);
    }

    public async Task<bool> PublishPostAsync(int postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        post.Status = PostStatus.Published;
        post.PublishedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

        await _postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> UnpublishPostAsync(int postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        post.Status = PostStatus.Draft;
        post.UpdatedAt = DateTime.UtcNow;

        await _postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> FeaturePostAsync(int postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        post.IsFeatured = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _postRepository.UpdateAsync(post);
        return true;
    }

    public async Task<bool> UnfeaturePostAsync(int postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        post.IsFeatured = false;
        post.UpdatedAt = DateTime.UtcNow;

        await _postRepository.UpdateAsync(post);
        return true;
    }

    #region Helper Methods

    private string GenerateSlug(string title)
    {
        // Convert to lowercase
        string slug = title.ToLowerInvariant();
        
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