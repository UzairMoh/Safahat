namespace Safahat.Application.DTOs.Requests.Posts;

public class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public bool IsDraft { get; set; } = true;
    public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    public List<string> Tags { get; set; } = new List<string>();
}