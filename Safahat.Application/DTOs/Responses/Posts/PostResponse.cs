using Safahat.Application.DTOs.Responses.Auth;
using Safahat.Application.DTOs.Responses.Categories;
using Safahat.Application.DTOs.Responses.Tags;
using Safahat.Models.Enums;

namespace Safahat.Application.DTOs.Responses.Posts;

public class PostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public string FeaturedImageUrl { get; set; }
    public PostStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public bool AllowComments { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
        
    // Related data
    public UserResponse Author { get; set; }
    public int CommentCount { get; set; }
    public List<CategoryResponse> Categories { get; set; } = new List<CategoryResponse>();
    public List<TagResponse> Tags { get; set; } = new List<TagResponse>();
}