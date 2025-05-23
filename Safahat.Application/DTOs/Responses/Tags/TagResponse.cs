namespace Safahat.Application.DTOs.Responses.Tags;

public class TagResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int PostCount { get; set; }
}