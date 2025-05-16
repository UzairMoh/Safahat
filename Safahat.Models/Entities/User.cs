using Safahat.Models.Enums;

namespace Safahat.Models.Entities;

public class User : BaseEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.Reader;
    public string Bio { get; set; }
    public string ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
        
    public ICollection<Post> Posts { get; set; }
    public ICollection<Comment> Comments { get; set; }
}