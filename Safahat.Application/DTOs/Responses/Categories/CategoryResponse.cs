namespace Safahat.Application.DTOs.Responses.Categories;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
    public int PostCount { get; set; }
}