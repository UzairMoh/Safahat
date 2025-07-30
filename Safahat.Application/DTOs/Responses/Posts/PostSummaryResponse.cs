using Safahat.Application.DTOs.Responses.Auth;

namespace Safahat.Application.DTOs.Responses.Posts;

public class PostSummaryResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
    public UserResponse Author { get; set; } = null!;
}