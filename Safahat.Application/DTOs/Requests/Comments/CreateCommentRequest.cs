namespace Safahat.Application.DTOs.Requests.Comments;

public class CreateCommentRequest
{
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }
    public string Content { get; set; }
}