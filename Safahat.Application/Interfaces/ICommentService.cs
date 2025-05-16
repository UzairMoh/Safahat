using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Responses.Comments;

namespace Safahat.Application.Interfaces;

public interface ICommentService
{
    // Basic CRUD operations
    Task<CommentResponse> GetByIdAsync(int id);
    Task<IEnumerable<CommentResponse>> GetAllAsync();
    Task<CommentResponse> CreateAsync(int userId, CreateCommentRequest request);
    Task<CommentResponse> UpdateAsync(int commentId, int userId, UpdateCommentRequest request);
    Task<bool> DeleteAsync(int commentId, int userId);
        
    // Specialized operations
    Task<IEnumerable<CommentResponse>> GetCommentsByPostAsync(int postId);
    Task<IEnumerable<CommentResponse>> GetCommentsByUserAsync(int userId);
    Task<IEnumerable<CommentResponse>> GetPendingCommentsAsync();
    Task<bool> ApproveCommentAsync(int commentId);
    Task<bool> RejectCommentAsync(int commentId);
    Task<CommentResponse> ReplyToCommentAsync(int parentCommentId, int userId, CreateCommentRequest request);
}