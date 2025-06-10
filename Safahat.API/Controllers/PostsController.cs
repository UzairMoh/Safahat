using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Posts;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

[Produces("application/json")]
public class PostsController(IPostService postService) : BaseController
{
    /// <summary>
    /// Retrieves all posts (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IEnumerable<PostResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetAllPosts()
    {
        var posts = await postService.GetAllAsync();
        return Ok(posts);
    }
    
    /// <summary>
    /// Retrieves published posts with pagination
    /// </summary>
    [HttpGet("published")]
    [ProducesResponseType(typeof(IEnumerable<PostResponse>), 200)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPublishedPosts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var posts = await postService.GetPublishedPostsAsync(pageNumber, pageSize);
        return Ok(posts);
    }
    
    /// <summary>
    /// Retrieves a specific post by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PostResponse>> GetPostById(Guid id)
    {
        try
        {
            var post = await postService.GetByIdAsync(id);
            return Ok(post);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Retrieves a specific post by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(PostResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PostResponse>> GetPostBySlug(string slug)
    {
        try
        {
            var post = await postService.GetBySlugAsync(slug);
            return Ok(post);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Retrieves posts by author with pagination
    /// </summary>
    [HttpGet("author/{authorId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<PostResponse>), 200)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPostsByAuthor(Guid authorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var isAuthorized = User.Identity.IsAuthenticated && (authorId == UserId || IsAdmin);
        var posts = await postService.GetPostsByAuthorAsync(authorId, pageNumber, pageSize);
        
        if (!isAuthorized)
        {
            posts = posts.Where(p => p.Status == Models.Enums.PostStatus.Published);
        }
        
        return Ok(posts);
    }
    
    /// <summary>
    /// Creates a new post
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(PostResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<PostResponse>> CreatePost([FromBody] CreatePostRequest request)
    {
        var post = await postService.CreateAsync(UserId, request);
        return Created($"/api/posts/{post.Id}", post);
    }
    
    /// <summary>
    /// Updates an existing post
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(PostResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PostResponse>> UpdatePost(Guid id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            var existingPost = await postService.GetByIdAsync(id);
            
            if (existingPost.Author.Id != UserId && !IsAdmin)
            {
                return Forbid("You don't have permission to edit this post");
            }
            
            var updatedPost = await postService.UpdateAsync(id, request);
            return Ok(updatedPost);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Deletes a post
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeletePost(Guid id)
    {
        try
        {
            var existingPost = await postService.GetByIdAsync(id);
            
            if (existingPost.Author.Id != UserId && !IsAdmin)
            {
                return Forbid("You don't have permission to delete this post");
            }
            
            await postService.DeleteAsync(id);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Searches posts by query with pagination
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<PostResponse>), 200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> SearchPosts([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrEmpty(query))
        {
            return BadRequest("Search query cannot be empty");
        }
        
        var posts = await postService.SearchPostsAsync(query, pageNumber, pageSize);
        return Ok(posts);
    }
    
    /// <summary>
    /// Retrieves posts by category with pagination
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<PostResponse>), 200)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPostsByCategory(Guid categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var posts = await postService.GetPostsByCategoryAsync(categoryId, pageNumber, pageSize);
        return Ok(posts);
    }
    
    /// <summary>
    /// Retrieves posts by tag with pagination
    /// </summary>
    [HttpGet("tag/{tagId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<PostResponse>), 200)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPostsByTag(Guid tagId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var posts = await postService.GetPostsByTagAsync(tagId, pageNumber, pageSize);
        return Ok(posts);
    }
    
    /// <summary>
    /// Get featured posts
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<PostResponse>), 200)]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetFeaturedPosts()
    {
        var posts = await postService.GetFeaturedPostsAsync();
        return Ok(posts);
    }
    
    /// <summary>
    /// Publishes a draft post
    /// </summary>
    [HttpPut("{id:guid}/publish")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> PublishPost(Guid id)
    {
        try
        {
            var existingPost = await postService.GetByIdAsync(id);
            
            if (existingPost.Author.Id != UserId && !IsAdmin)
            {
                return Forbid("You don't have permission to publish this post");
            }
            
            await postService.PublishPostAsync(id);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Unpublishes a published post
    /// </summary>
    [HttpPut("{id:guid}/unpublish")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> UnpublishPost(Guid id)
    {
        try
        {
            var existingPost = await postService.GetByIdAsync(id);
            
            if (existingPost.Author.Id != UserId && !IsAdmin)
            {
                return Forbid("You don't have permission to unpublish this post");
            }
            
            await postService.UnpublishPostAsync(id);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Features a post (Admin only)
    /// </summary>
    [HttpPut("{id:guid}/feature")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> FeaturePost(Guid id)
    {
        try
        {
            await postService.FeaturePostAsync(id);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    /// <summary>
    /// Unfeatures a post (Admin only)
    /// </summary>
    [HttpPut("{id:guid}/unfeature")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> UnfeaturePost(Guid id)
    {
        try
        {
            await postService.UnfeaturePostAsync(id);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}