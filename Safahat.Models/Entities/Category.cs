using System.ComponentModel.DataAnnotations;

namespace Safahat.Models.Entities;

/// <summary>
/// Represents a category for organizing blog posts
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// The display name of the category
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SEO-friendly URL slug for the category
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the category
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    // Navigation properties

    /// <summary>
    /// Collection of post-category associations
    /// </summary>
    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
}