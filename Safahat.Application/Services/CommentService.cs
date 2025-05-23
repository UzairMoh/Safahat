using AutoMapper;
using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Responses.Comments;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Application.Services;

public class CommentService(
    ICommentRepository commentRepository,
    IPostRepository postRepository,
    IUserRepository userRepository,
    IMapper mapper)
    : ICommentService
{
    public async Task<CommentResponse> GetByIdAsync(Guid id)
    {
        var comment = await commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        return mapper.Map<CommentResponse>(comment);
    }

    public async Task<IEnumerable<CommentResponse>> GetAllAsync()
    {
        var comments = await commentRepository.GetAllAsync();
        return mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<CommentResponse> CreateAsync(Guid userId, CreateCommentRequest request)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var post = await postRepository.GetByIdAsync(request.PostId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        if (!post.AllowComments)
        {
            throw new ApplicationException("Comments are not allowed for this post");
        }

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await commentRepository.GetByIdAsync(request.ParentCommentId.Value);
            if (parentComment == null)
            {
                throw new ApplicationException("Parent comment not found");
            }

            if (parentComment.PostId != request.PostId)
            {
                throw new ApplicationException("Parent comment does not belong to the specified post");
            }
        }

        var comment = mapper.Map<Comment>(request);
        comment.UserId = userId;
        comment.IsApproved = true;

        var createdComment = await commentRepository.AddAsync(comment);
        return mapper.Map<CommentResponse>(createdComment);
    }

    public async Task<CommentResponse> UpdateAsync(Guid commentId, Guid userId, UpdateCommentRequest request)
    {
        var comment = await commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        if (comment.UserId != userId)
        {
            throw new ApplicationException("You are not authorized to update this comment");
        }

        mapper.Map(request, comment);
        comment.UpdatedAt = DateTime.UtcNow;
        comment.IsApproved = false;

        await commentRepository.UpdateAsync(comment);
        return mapper.Map<CommentResponse>(comment);
    }

    public async Task<bool> DeleteAsync(Guid commentId, Guid userId)
    {
        var comment = await commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        if (comment.UserId != userId)
        {
            throw new ApplicationException("You are not authorized to delete this comment");
        }

        await commentRepository.DeleteAsync(commentId);
        return true;
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsByPostAsync(Guid postId)
    {
        var comments = await commentRepository.GetCommentsByPostAsync(postId);
        return mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsByUserAsync(Guid userId)
    {
        var comments = await commentRepository.GetCommentsByUserAsync(userId);
        return mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<IEnumerable<CommentResponse>> GetPendingCommentsAsync()
    {
        var comments = await commentRepository.GetPendingCommentsAsync();
        return mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<bool> ApproveCommentAsync(Guid commentId)
    {
        var comment = await commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        comment.IsApproved = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await commentRepository.UpdateAsync(comment);
        return true;
    }

    public async Task<bool> RejectCommentAsync(Guid commentId)
    {
        var comment = await commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        await commentRepository.DeleteAsync(commentId);
        return true;
    }

    public async Task<CommentResponse> ReplyToCommentAsync(Guid parentCommentId, Guid userId, CreateCommentRequest request)
    {
        var parentComment = await commentRepository.GetByIdAsync(parentCommentId);
        if (parentComment == null)
        {
            throw new ApplicationException("Parent comment not found");
        }

        request.PostId = parentComment.PostId;
        request.ParentCommentId = parentCommentId;
        
        return await CreateAsync(userId, request);
    }
}