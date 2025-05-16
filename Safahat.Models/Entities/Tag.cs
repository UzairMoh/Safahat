namespace Safahat.Models.Entities;

public class Tag : BaseEntity
{
    public string Name { get; set; }
    public string Slug { get; set; }
        
    // Navigation properties
    public ICollection<PostTag> PostTags { get; set; }
}