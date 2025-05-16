namespace Safahat.Application.DTOs.Responses.Comments;

public class CommentResponse
{
    public int Id { get; set; }
    public string Content { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
        
    // Related data
    public int PostId { get; set; }
    public string PostTitle { get; set; }
    public UserResponse User { get; set; }
    public int? ParentCommentId { get; set; }
    public List<CommentResponse> Replies { get; set; } = new List<CommentResponse>();
}