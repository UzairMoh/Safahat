namespace Safahat.Application.DTOs.Requests.Comments;

public class CreateCommentRequest
{
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; }
}