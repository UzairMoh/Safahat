using Safahat.Application.DTOs.Responses.Auth;

namespace Safahat.Application.DTOs.Responses.Posts;

public class PostSummaryResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Summary { get; set; }
    public string FeaturedImageUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public int CommentCount { get; set; }
    public UserResponse Author { get; set; }
}