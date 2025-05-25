using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Safahat.Models.Entities;

/// <summary>
/// Junction entity representing the many-to-many relationship between Posts and Tags
/// </summary>
public class PostTag
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
    /// Foreign key reference to the tag
    /// </summary>
    [Required]
    public Guid TagId { get; set; }

    /// <summary>
    /// The tag in this relationship
    /// </summary>
    [ForeignKey(nameof(TagId))]
    public virtual Tag Tag { get; set; } = null!;
}