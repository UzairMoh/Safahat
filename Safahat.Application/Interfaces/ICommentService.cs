using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Responses.Comments;

namespace Safahat.Application.Interfaces;

public interface ICommentService
{
    Task<CommentResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<CommentResponse>> GetAllAsync();
    Task<CommentResponse> CreateAsync(Guid userId, CreateCommentRequest request);
    Task<CommentResponse> UpdateAsync(Guid commentId, Guid userId, UpdateCommentRequest request);
    Task<bool> DeleteAsync(Guid commentId, Guid userId);
    Task<IEnumerable<CommentResponse>> GetCommentsByPostAsync(Guid postId);
    Task<IEnumerable<CommentResponse>> GetCommentsByUserAsync(Guid userId);
    Task<IEnumerable<CommentResponse>> GetPendingCommentsAsync();
    Task<bool> ApproveCommentAsync(Guid commentId);
    Task<bool> RejectCommentAsync(Guid commentId);
    Task<CommentResponse> ReplyToCommentAsync(Guid parentCommentId, Guid userId, CreateCommentRequest request);
}