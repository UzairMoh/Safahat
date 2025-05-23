namespace Safahat.Application.DTOs.Requests.Posts;

public class CreatePostRequest
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public string FeaturedImageUrl { get; set; }
    public bool AllowComments { get; set; } = true;
    public bool IsDraft { get; set; } = true;
    public List<Guid> CategoryIds { get; set; } = new List<Guid>();
    public List<string> Tags { get; set; } = new List<string>();
}