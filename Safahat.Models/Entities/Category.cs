namespace Safahat.Models.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; }
    public string Slug { get; set; }
    public string Description { get; set; }
        
    // Navigation properties
    public ICollection<PostCategory> PostCategories { get; set; }
}