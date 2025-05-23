using Safahat.Models.Enums;

namespace Safahat.Application.DTOs.Responses.Users;

public class UserDetailResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }
    public string Bio { get; set; }
    public string ProfilePictureUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
        
    // Statistics
    public int PostCount { get; set; }
    public int CommentCount { get; set; }
}