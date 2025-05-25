using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Safahat.Models.Enums;

namespace Safahat.Models.Entities;

/// <summary>
/// Represents a user in the blogging platform (authors and readers)
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Unique username for the user
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's email address (used for authentication)
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication
    /// </summary>
    [Required]
    [StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's role in the system (Reader, Author, Admin)
    /// </summary>
    [Required]
    public UserRole Role { get; set; } = UserRole.Reader;

    /// <summary>
    /// Optional biographical information about the user
    /// </summary>
    [StringLength(1000)]
    public string? Bio { get; set; }

    /// <summary>
    /// URL to the user's profile picture
    /// </summary>
    [StringLength(500)]
    [Url]
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Indicates whether the user account is active
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time of the user's last login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties

    /// <summary>
    /// Collection of posts authored by this user
    /// </summary>
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    /// <summary>
    /// Collection of comments made by this user
    /// </summary>
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}