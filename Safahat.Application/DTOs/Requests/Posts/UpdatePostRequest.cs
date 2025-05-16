namespace Safahat.Application.DTOs.Requests.Posts;

public class UpdatePostRequest
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public string FeaturedImageUrl { get; set; }
    public bool AllowComments { get; set; }
    public List<int> CategoryIds { get; set; } = new List<int>();
    public List<string> Tags { get; set; } = new List<string>();
}