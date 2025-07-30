using Safahat.Application.DTOs.Responses.Auth;

namespace Safahat.Application.DTOs.Responses.Comments;

public class CommentResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
        
    // Related data
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public UserResponse User { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
}