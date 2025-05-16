using AutoMapper;
using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Responses.Comments;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<CommentResponse> GetByIdAsync(int id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        return _mapper.Map<CommentResponse>(comment);
    }

    public async Task<IEnumerable<CommentResponse>> GetAllAsync()
    {
        var comments = await _commentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<CommentResponse> CreateAsync(int userId, CreateCommentRequest request)
    {
        // Verify user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        // Verify post exists
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post == null)
        {
            throw new ApplicationException("Post not found");
        }

        // Check if post allows comments
        if (!post.AllowComments)
        {
            throw new ApplicationException("Comments are not allowed for this post");
        }

        // Check parent comment if specified
        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId.Value);
            if (parentComment == null)
            {
                throw new ApplicationException("Parent comment not found");
            }

            // Ensure parent comment belongs to the same post
            if (parentComment.PostId != request.PostId)
            {
                throw new ApplicationException("Parent comment does not belong to the specified post");
            }
        }

        var comment = _mapper.Map<Comment>(request);
        comment.UserId = userId;
        
        // Auto-approve comments for now (or implement moderation logic)
        comment.IsApproved = true;

        var createdComment = await _commentRepository.AddAsync(comment);
        return _mapper.Map<CommentResponse>(createdComment);
    }

    public async Task<CommentResponse> UpdateAsync(int commentId, int userId, UpdateCommentRequest request)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        // Check if user is the author of the comment
        if (comment.UserId != userId)
        {
            throw new ApplicationException("You are not authorized to update this comment");
        }

        // Update comment properties
        _mapper.Map(request, comment);
        comment.UpdatedAt = DateTime.UtcNow;
        
        // Reset approval status when content is updated
        comment.IsApproved = false;

        await _commentRepository.UpdateAsync(comment);
        return _mapper.Map<CommentResponse>(comment);
    }

    public async Task<bool> DeleteAsync(int commentId, int userId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        // Check if user is the author of the comment
        if (comment.UserId != userId)
        {
            // Check if user is admin or post author (implement role check)
            throw new ApplicationException("You are not authorized to delete this comment");
        }

        await _commentRepository.DeleteAsync(commentId);
        return true;
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsByPostAsync(int postId)
    {
        var comments = await _commentRepository.GetCommentsByPostAsync(postId);
        return _mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsByUserAsync(int userId)
    {
        var comments = await _commentRepository.GetCommentsByUserAsync(userId);
        return _mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<IEnumerable<CommentResponse>> GetPendingCommentsAsync()
    {
        var comments = await _commentRepository.GetPendingCommentsAsync();
        return _mapper.Map<IEnumerable<CommentResponse>>(comments);
    }

    public async Task<bool> ApproveCommentAsync(int commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        comment.IsApproved = true;
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);
        return true;
    }

    public async Task<bool> RejectCommentAsync(int commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
        {
            throw new ApplicationException("Comment not found");
        }

        // Option 1: Delete rejected comments
        await _commentRepository.DeleteAsync(commentId);
        
        // Option 2: Mark as rejected but keep in database
        // comment.IsApproved = false;
        // comment.UpdatedAt = DateTime.UtcNow;
        // await _commentRepository.UpdateAsync(comment);

        return true;
    }

    public async Task<CommentResponse> ReplyToCommentAsync(int parentCommentId, int userId, CreateCommentRequest request)
    {
        // Verify parent comment exists
        var parentComment = await _commentRepository.GetByIdAsync(parentCommentId);
        if (parentComment == null)
        {
            throw new ApplicationException("Parent comment not found");
        }

        // Create reply
        request.PostId = parentComment.PostId;
        request.ParentCommentId = parentCommentId;
        
        return await CreateAsync(userId, request);
    }
}