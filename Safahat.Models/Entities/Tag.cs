using System.ComponentModel.DataAnnotations;

namespace Safahat.Models.Entities;

/// <summary>
/// Represents a tag for labeling and organizing blog posts
/// </summary>
public class Tag : BaseEntity
{
    /// <summary>
    /// The display name of the tag
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SEO-friendly URL slug for the tag
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Slug { get; set; } = string.Empty;

    // Navigation properties

    /// <summary>
    /// Collection of post-tag associations
    /// </summary>
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}