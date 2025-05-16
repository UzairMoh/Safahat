namespace Safahat.Models.Entities;

public class Comment : BaseEntity
{
    public string Content { get; set; }
    public bool IsApproved { get; set; } = false;
        
    // Foreign keys
    public int PostId { get; set; }
    public int UserId { get; set; }
    public int? ParentCommentId { get; set; }
        
    // Navigation properties
    public Post Post { get; set; }
    public User User { get; set; }
    public Comment ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; }
}