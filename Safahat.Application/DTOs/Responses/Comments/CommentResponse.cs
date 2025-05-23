using Safahat.Application.DTOs.Responses.Auth;

namespace Safahat.Application.DTOs.Responses.Comments;

public class CommentResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
        
    // Related data
    public Guid PostId { get; set; }
    public string PostTitle { get; set; }
    public UserResponse User { get; set; }
    public Guid? ParentCommentId { get; set; }
    public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
}