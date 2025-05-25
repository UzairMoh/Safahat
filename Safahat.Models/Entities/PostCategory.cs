using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Safahat.Models.Entities;

/// <summary>
/// Junction entity representing the many-to-many relationship between Posts and Categories
/// </summary>
public class PostCategory
{
    /// <summary>
    /// Foreign key reference to the post
    /// </summary>
    [Required]
    public Guid PostId { get; set; }

    /// <summary>
    /// The post in this relationship
    /// </summary>
    [ForeignKey(nameof(PostId))]
    public virtual Post Post { get; set; } = null!;

    /// <summary>
    /// Foreign key reference to the category
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// The category in this relationship
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; } = null!;
}