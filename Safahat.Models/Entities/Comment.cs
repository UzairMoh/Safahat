using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Safahat.Models.Entities;

/// <summary>
/// Represents a comment on a blog post with support for hierarchical replies
/// </summary>
public class Comment : BaseEntity
{
    /// <summary>
    /// The content/text of the comment
    /// </summary>
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    // Foreign keys

    /// <summary>
    /// Foreign key reference to the post this comment belongs to
    /// </summary>
    [Required]
    public Guid PostId { get; set; }

    /// <summary>
    /// Foreign key reference to the user who made this comment
    /// </summary>
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// Foreign key reference to the parent comment (null for top-level comments)
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    // Navigation properties

    /// <summary>
    /// The post this comment belongs to
    /// </summary>
    [ForeignKey(nameof(PostId))]
    public virtual Post Post { get; set; } = null!;

    /// <summary>
    /// The user who made this comment
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// The parent comment (null for top-level comments)
    /// </summary>
    [ForeignKey(nameof(ParentCommentId))]
    public virtual Comment? ParentComment { get; set; }

    /// <summary>
    /// Collection of replies to this comment
    /// </summary>
    [InverseProperty(nameof(ParentComment))]
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}