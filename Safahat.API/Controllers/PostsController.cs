using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Posts;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

public class PostsController(IPostService postService) : BaseController
{
    /// <summary>
    /// Get all posts - Admin only
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetAllPosts()
    {
        var posts = await postService.GetAllAsync();
        return Success(posts);
    }
    
    /// <summary>
    /// Get published posts with pagination
    /// </summary>
    [HttpGet("published")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPublishedPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var posts = await postService.GetPublishedPostsAsync(pageNumber, pageSize);
        return Success(posts);
    }
    
    /// <summary>
    /// Get post by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostResponse>> GetPostById(Guid id)
    {
        try
        {
            var post = await postService.GetByIdAsync(id);
            return Success(post);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
    
    /// <summary>
    /// Get post by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<PostResponse>> GetPostBySlug(string slug)
    {
        try
        {
            var post = await postService.GetBySlugAsync(slug);
            return Success(post);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
    
    /// <summary>
    /// Get posts by author - With authorization check
    /// </summary>
    [HttpGet("author/{authorId:guid}")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPostsByAuthor(Guid authorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var isAuthorized = User.Identity.IsAuthenticated && (authorId == UserId || IsAdmin);
        var posts = await postService.GetPostsByAuthorAsync(authorId, pageNumber, pageSize);
        
        if (!isAuthorized)
        {
            posts = posts.Where(p => p.Status == Models.Enums.PostStatus.Published);
        }
        
        return Success(posts);
    }
    
    /// <summary>
    /// Create a post - Authenticated users only
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<PostResponse>> CreatePost([FromBody] CreatePostRequest request)
    {
        var post = await postService.CreateAsync(UserId, request);
        return CreatedWithMessage("Post created successfully", post);
    }
    
    /// <summary>
    /// Update a post - Author or Admin only
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<PostResponse>> UpdatePost(Guid id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            var existingPost = await postService.GetByIdAsync(id);
            
            if (existingPost.Author.Id != UserId && !IsAdmin)
            {
                return ForbidWithMessage("You don't have permission to edit this post");
            }
            
            var updatedPost = await postService.UpdateAsync(id, request);
            return Success(updatedPost);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
    
    /// <summary>
    /// Delete a post - Author or Admin only
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult> DeletePost(Guid id)
    {
        try
        {
            var existingPost = await postService.GetByIdAsync(id);
            
            if (existingPost.Author.Id != UserId && !IsAdmin)
            {
                return ForbidWithMessage("You don't have permission to delete this post");
            }
            
            var result = await postService.DeleteAsync(id);
            return Success(new { deleted = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
    
    /// <summary>
    /// Search posts
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> SearchPosts([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrEmpty(query))
        {
            return BadRequestWithMessage("Search query cannot be empty");
        }
        
        var posts = await postService.SearchPostsAsync(query, pageNumber, pageSize);
        return Success(posts);
    }
    
    /// <summary>
    /// Get posts by category
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPostsByCategory(Guid categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var posts = await postService.GetPostsByCategoryAsync(categoryId, pageNumber, pageSize);
        return Success(posts);
    }
    
    /// <summary>
    /// Get posts by tag
    /// </summary>
    [HttpGet("tag/{tagId:guid}")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPostsByTag(Guid tagId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var posts = await postService.GetPostsByTagAsync(tagId, pageNumber, pageSize);
        return Success(posts);
    }
    
    /// <summary>
    /// Get featured posts
    /// </summary>
    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetFeaturedPosts()
    {
        var posts = await postService.GetFeaturedPostsAsync();
        return Success(posts);
    }
    
    /// <summary>
    /// Publish a post - Author or Admin only
    /// </summary>
    [HttpPut("{id:guid}/publish")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult> PublishPost(Guid id)
    {
        try
        {
            var existingPost = await postService.GetByIdAsync(id);
            
            if (existingPost.Author.Id != UserId && !IsAdmin)
            {
                return ForbidWithMessage("You don't have permission to publish this post");
            }
            
            var result = await postService.PublishPostAsync(id);
            return Success(new { published = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
    
    /// <summary>
    /// Unpublish a post - Author or Admin only
    /// </summary>
    [HttpPut("{id:guid}/unpublish")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult> UnpublishPost(Guid id)
    {
        try
        {
            var existingPost = await postService.GetByIdAsync(id);
            
            if (existingPost.Author.Id != UserId && !IsAdmin)
            {
                return ForbidWithMessage("You don't have permission to unpublish this post");
            }
            
            var result = await postService.UnpublishPostAsync(id);
            return Success(new { unpublished = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
    
    /// <summary>
    /// Feature a post - Admin only
    /// </summary>
    [HttpPut("{id:guid}/feature")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> FeaturePost(Guid id)
    {
        try
        {
            var result = await postService.FeaturePostAsync(id);
            return Success(new { featured = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
    
    /// <summary>
    /// Unfeature a post - Admin only
    /// </summary>
    [HttpPut("{id:guid}/unfeature")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> UnfeaturePost(Guid id)
    {
        try
        {
            var result = await postService.UnfeaturePostAsync(id);
            return Success(new { unfeatured = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
}