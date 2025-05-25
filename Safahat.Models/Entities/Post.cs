using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Safahat.Models.Enums;

namespace Safahat.Models.Entities;

/// <summary>
/// Represents a blog post in the platform
/// </summary>
public class Post : BaseEntity
{
    /// <summary>
    /// The title of the blog post
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// SEO-friendly URL slug for the post
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// The main content/body of the blog post
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Brief summary or excerpt of the post
    /// </summary>
    [Required]
    [StringLength(500)]
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// URL to the featured image for the post
    /// </summary>
    [StringLength(500)]
    [Url]
    public string? FeaturedImageUrl { get; set; }

    /// <summary>
    /// Current status of the post (Draft, Published, Archived)
    /// </summary>
    [Required]
    public PostStatus Status { get; set; } = PostStatus.Draft;

    /// <summary>
    /// Date and time when the post was published
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Number of times this post has been viewed
    /// </summary>
    [Required]
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// Indicates whether comments are allowed on this post
    /// </summary>
    [Required]
    public bool AllowComments { get; set; } = true;

    /// <summary>
    /// Indicates whether this post is featured
    /// </summary>
    [Required]
    public bool IsFeatured { get; set; } = false;

    // Foreign keys

    /// <summary>
    /// Foreign key reference to the author (User) of this post
    /// </summary>
    [Required]
    public Guid AuthorId { get; set; }

    // Navigation properties

    /// <summary>
    /// The author (User) who created this post
    /// </summary>
    [ForeignKey(nameof(AuthorId))]
    public virtual User Author { get; set; } = null!;

    /// <summary>
    /// Collection of comments on this post
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    /// <summary>
    /// Collection of category associations for this post
    /// </summary>
    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();

    /// <summary>
    /// Collection of tag associations for this post
    /// </summary>
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}