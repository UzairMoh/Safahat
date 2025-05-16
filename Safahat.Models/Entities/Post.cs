using Safahat.Models.Enums;

namespace Safahat.Models.Entities;

public class Post : BaseEntity
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public string Summary { get; set; }
    public string FeaturedImageUrl { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; } = 0;
    public bool AllowComments { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
        
    // Foreign keys
    public int AuthorId { get; set; }
        
    // Navigation properties
    public User Author { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<PostCategory> PostCategories { get; set; }
    public ICollection<PostTag> PostTags { get; set; }
}