using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Responses.Auth;
using Safahat.Application.DTOs.Responses.Comments;
using Safahat.Application.Services;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Tests.Services;

public class CommentServiceTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        
        _commentService = new CommentService(
            _commentRepository,
            _postRepository,
            _userRepository,
            _mapper
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingComment_ShouldReturnMappedCommentResponse()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = commentId,
            Content = "Test comment",
            UserId = Guid.NewGuid(),
            PostId = Guid.NewGuid()
        };

        var expectedResponse = new CommentResponse
        {
            Id = commentId,
            Content = "Test comment"
        };

        _commentRepository.GetByIdAsync(commentId).Returns(comment);
        _mapper.Map<CommentResponse>(comment).Returns(expectedResponse);

        // Act
        var result = await _commentService.GetByIdAsync(commentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _commentRepository.Received(1).GetByIdAsync(commentId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentComment_ShouldThrowApplicationException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _commentRepository.GetByIdAsync(commentId).Returns((Comment)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.GetByIdAsync(commentId)
        );

        exception.Message.Should().Be("Comment not found");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedCommentsList()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), Content = "Comment 1" },
            new Comment { Id = Guid.NewGuid(), Content = "Comment 2" }
        };

        var expectedResponse = new List<CommentResponse>
        {
            new CommentResponse { Id = comments[0].Id, Content = "Comment 1" },
            new CommentResponse { Id = comments[1].Id, Content = "Comment 2" }
        };

        _commentRepository.GetAllAsync().Returns(comments);
        _mapper.Map<IEnumerable<CommentResponse>>(comments).Returns(expectedResponse);

        // Act
        var result = await _commentService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _commentRepository.Received(1).GetAllAsync();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateCommentAndReturnResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            PostId = postId,
            Content = "Test comment content"
        };

        var user = new User { Id = userId, Username = "testuser", Email = "test@example.com" };
        var post = new Post { Id = postId, AllowComments = true };
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            PostId = postId,
            UserId = userId,
            IsApproved = true
        };

        var userResponse = new UserResponse
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com"
        };

        var expectedResponse = new CommentResponse
        {
            Id = comment.Id,
            Content = request.Content,
            PostId = postId,
            User = userResponse,
            IsApproved = true
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetByIdAsync(postId).Returns(post);
        _mapper.Map<Comment>(request).Returns(comment);
        _commentRepository.AddAsync(Arg.Any<Comment>()).Returns(comment);
        _mapper.Map<CommentResponse>(comment).Returns(expectedResponse);

        // Act
        var result = await _commentService.CreateAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(userId);
        result.User.Username.Should().Be("testuser");
        
        await _commentRepository.Received(1).AddAsync(Arg.Is<Comment>(c => 
            c.UserId == userId && 
            c.PostId == postId &&
            c.IsApproved == true));
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest { PostId = Guid.NewGuid(), Content = "Test" };

        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.CreateAsync(userId, request)
        );

        exception.Message.Should().Be("User not found");
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentPost_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var request = new CreateCommentRequest { PostId = postId, Content = "Test" };

        var user = new User { Id = userId };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetByIdAsync(postId).Returns((Post)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.CreateAsync(userId, request)
        );

        exception.Message.Should().Be("Post not found");
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task CreateAsync_WithPostNotAllowingComments_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var request = new CreateCommentRequest { PostId = postId, Content = "Test" };

        var user = new User { Id = userId };
        var post = new Post { Id = postId, AllowComments = false };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetByIdAsync(postId).Returns(post);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.CreateAsync(userId, request)
        );

        exception.Message.Should().Be("Comments are not allowed for this post");
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task CreateAsync_WithValidParentComment_ShouldCreateReplyComment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            PostId = postId,
            Content = "Reply comment",
            ParentCommentId = parentCommentId
        };

        var user = new User { Id = userId };
        var post = new Post { Id = postId, AllowComments = true };
        var parentComment = new Comment { Id = parentCommentId, PostId = postId };
        var comment = new Comment { Id = Guid.NewGuid(), Content = request.Content };
        var expectedResponse = new CommentResponse { Id = comment.Id };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetByIdAsync(postId).Returns(post);
        _commentRepository.GetByIdAsync(parentCommentId).Returns(parentComment);
        _mapper.Map<Comment>(request).Returns(comment);
        _commentRepository.AddAsync(Arg.Any<Comment>()).Returns(comment);
        _mapper.Map<CommentResponse>(comment).Returns(expectedResponse);

        // Act
        var result = await _commentService.CreateAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        await _commentRepository.Received(1).GetByIdAsync(parentCommentId);
        await _commentRepository.Received(1).AddAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentParentComment_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            PostId = postId,
            Content = "Reply comment",
            ParentCommentId = parentCommentId
        };

        var user = new User { Id = userId };
        var post = new Post { Id = postId, AllowComments = true };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetByIdAsync(postId).Returns(post);
        _commentRepository.GetByIdAsync(parentCommentId).Returns((Comment)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.CreateAsync(userId, request)
        );

        exception.Message.Should().Be("Parent comment not found");
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task CreateAsync_WithParentCommentFromDifferentPost_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            PostId = postId,
            Content = "Reply comment",
            ParentCommentId = parentCommentId
        };

        var user = new User { Id = userId };
        var post = new Post { Id = postId, AllowComments = true };
        var parentComment = new Comment { Id = parentCommentId, PostId = Guid.NewGuid() }; // Different post

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetByIdAsync(postId).Returns(post);
        _commentRepository.GetByIdAsync(parentCommentId).Returns(parentComment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.CreateAsync(userId, request)
        );

        exception.Message.Should().Be("Parent comment does not belong to the specified post");
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateCommentAndReturnResponse()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Content = "Updated content" };

        var comment = new Comment
        {
            Id = commentId,
            UserId = userId,
            Content = "Original content",
            IsApproved = true
        };

        var expectedResponse = new CommentResponse
        {
            Id = commentId,
            Content = "Updated content"
        };

        _commentRepository.GetByIdAsync(commentId).Returns(comment);
        _mapper.Map<CommentResponse>(comment).Returns(expectedResponse);

        // Act
        var result = await _commentService.UpdateAsync(commentId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        
        await _commentRepository.Received(1).UpdateAsync(Arg.Is<Comment>(c => 
            c.Id == commentId && 
            c.IsApproved == false &&
            c.UpdatedAt != null));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentComment_ShouldThrowApplicationException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Content = "Updated content" };

        _commentRepository.GetByIdAsync(commentId).Returns((Comment)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.UpdateAsync(commentId, userId, request)
        );

        exception.Message.Should().Be("Comment not found");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
    }

    [Fact]
    public async Task UpdateAsync_WithUnauthorizedUser_ShouldThrowApplicationException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Content = "Updated content" };

        var comment = new Comment
        {
            Id = commentId,
            UserId = differentUserId, // Different user
            Content = "Original content"
        };

        _commentRepository.GetByIdAsync(commentId).Returns(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.UpdateAsync(commentId, userId, request)
        );

        exception.Message.Should().Be("You are not authorized to update this comment");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidData_ShouldDeleteCommentAndReturnTrue()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            UserId = userId,
            Content = "Test comment"
        };

        _commentRepository.GetByIdAsync(commentId).Returns(comment);

        // Act
        var result = await _commentService.DeleteAsync(commentId, userId);

        // Assert
        result.Should().BeTrue();
        await _commentRepository.Received(1).DeleteAsync(commentId);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentComment_ShouldThrowApplicationException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _commentRepository.GetByIdAsync(commentId).Returns((Comment)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.DeleteAsync(commentId, userId)
        );

        exception.Message.Should().Be("Comment not found");
        await _commentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task DeleteAsync_WithUnauthorizedUser_ShouldThrowApplicationException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            UserId = differentUserId, // Different user
            Content = "Test comment"
        };

        _commentRepository.GetByIdAsync(commentId).Returns(comment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.DeleteAsync(commentId, userId)
        );

        exception.Message.Should().Be("You are not authorized to delete this comment");
        await _commentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    #endregion

    #region GetCommentsByPostAsync Tests

    [Fact]
    public async Task GetCommentsByPostAsync_ShouldReturnMappedCommentsList()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), PostId = postId, Content = "Comment 1" },
            new Comment { Id = Guid.NewGuid(), PostId = postId, Content = "Comment 2" }
        };

        var expectedResponse = new List<CommentResponse>
        {
            new CommentResponse { Id = comments[0].Id, Content = "Comment 1" },
            new CommentResponse { Id = comments[1].Id, Content = "Comment 2" }
        };

        _commentRepository.GetCommentsByPostAsync(postId).Returns(comments);
        _mapper.Map<IEnumerable<CommentResponse>>(comments).Returns(expectedResponse);

        // Act
        var result = await _commentService.GetCommentsByPostAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _commentRepository.Received(1).GetCommentsByPostAsync(postId);
    }

    #endregion

    #region GetCommentsByUserAsync Tests

    [Fact]
    public async Task GetCommentsByUserAsync_ShouldReturnMappedCommentsList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), UserId = userId, Content = "Comment 1" },
            new Comment { Id = Guid.NewGuid(), UserId = userId, Content = "Comment 2" }
        };

        var expectedResponse = new List<CommentResponse>
        {
            new CommentResponse { Id = comments[0].Id, Content = "Comment 1" },
            new CommentResponse { Id = comments[1].Id, Content = "Comment 2" }
        };

        _commentRepository.GetCommentsByUserAsync(userId).Returns(comments);
        _mapper.Map<IEnumerable<CommentResponse>>(comments).Returns(expectedResponse);

        // Act
        var result = await _commentService.GetCommentsByUserAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _commentRepository.Received(1).GetCommentsByUserAsync(userId);
    }

    #endregion

    #region GetPendingCommentsAsync Tests

    [Fact]
    public async Task GetPendingCommentsAsync_ShouldReturnMappedPendingCommentsList()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), Content = "Pending comment 1", IsApproved = false },
            new Comment { Id = Guid.NewGuid(), Content = "Pending comment 2", IsApproved = false }
        };

        var expectedResponse = new List<CommentResponse>
        {
            new CommentResponse { Id = comments[0].Id, Content = "Pending comment 1" },
            new CommentResponse { Id = comments[1].Id, Content = "Pending comment 2" }
        };

        _commentRepository.GetPendingCommentsAsync().Returns(comments);
        _mapper.Map<IEnumerable<CommentResponse>>(comments).Returns(expectedResponse);

        // Act
        var result = await _commentService.GetPendingCommentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _commentRepository.Received(1).GetPendingCommentsAsync();
    }

    #endregion

    #region ApproveCommentAsync Tests

    [Fact]
    public async Task ApproveCommentAsync_WithExistingComment_ShouldApproveAndReturnTrue()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = commentId,
            Content = "Test comment",
            IsApproved = false
        };

        _commentRepository.GetByIdAsync(commentId).Returns(comment);

        // Act
        var result = await _commentService.ApproveCommentAsync(commentId);

        // Assert
        result.Should().BeTrue();
        
        await _commentRepository.Received(1).UpdateAsync(Arg.Is<Comment>(c => 
            c.Id == commentId && 
            c.IsApproved == true &&
            c.UpdatedAt != null));
    }

    [Fact]
    public async Task ApproveCommentAsync_WithNonExistentComment_ShouldThrowApplicationException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _commentRepository.GetByIdAsync(commentId).Returns((Comment)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.ApproveCommentAsync(commentId)
        );

        exception.Message.Should().Be("Comment not found");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
    }

    #endregion

    #region RejectCommentAsync Tests

    [Fact]
    public async Task RejectCommentAsync_WithExistingComment_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var comment = new Comment
        {
            Id = commentId,
            Content = "Test comment",
            IsApproved = false
        };

        _commentRepository.GetByIdAsync(commentId).Returns(comment);

        // Act
        var result = await _commentService.RejectCommentAsync(commentId);

        // Assert
        result.Should().BeTrue();
        await _commentRepository.Received(1).DeleteAsync(commentId);
    }

    [Fact]
    public async Task RejectCommentAsync_WithNonExistentComment_ShouldThrowApplicationException()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        _commentRepository.GetByIdAsync(commentId).Returns((Comment)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.RejectCommentAsync(commentId)
        );

        exception.Message.Should().Be("Comment not found");
        await _commentRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    #endregion

    #region ReplyToCommentAsync Tests

    [Fact]
    public async Task ReplyToCommentAsync_WithValidParentComment_ShouldCreateReplyAndReturnResponse()
    {
        // Arrange
        var parentCommentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var request = new CreateCommentRequest { Content = "Reply content" };

        var parentComment = new Comment
        {
            Id = parentCommentId,
            PostId = postId,
            Content = "Parent comment"
        };

        var user = new User { Id = userId };
        var post = new Post { Id = postId, AllowComments = true };
        
        // Create the reply comment with the expected properties set
        var replyComment = new Comment 
        { 
            Id = Guid.NewGuid(), 
            Content = request.Content,
            UserId = userId,  // This will be set by CreateAsync
            PostId = postId,  // This will be set by CreateAsync
            ParentCommentId = parentCommentId,  // This will be set by CreateAsync
            IsApproved = true  // This will be set by CreateAsync
        };
        
        var expectedResponse = new CommentResponse { Id = replyComment.Id, Content = request.Content };

        _commentRepository.GetByIdAsync(parentCommentId).Returns(parentComment);
        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetByIdAsync(postId).Returns(post);
        
        // Mock the mapper to return the comment with proper properties when mapping the modified request
        _mapper.Map<Comment>(Arg.Is<CreateCommentRequest>(r => 
            r.PostId == postId && 
            r.ParentCommentId == parentCommentId && 
            r.Content == "Reply content")).Returns(replyComment);
            
        _commentRepository.AddAsync(Arg.Any<Comment>()).Returns(replyComment);
        _mapper.Map<CommentResponse>(replyComment).Returns(expectedResponse);

        // Act
        var result = await _commentService.ReplyToCommentAsync(parentCommentId, userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        
        // Verify that the comment was added with the correct properties
        await _commentRepository.Received(1).AddAsync(Arg.Is<Comment>(c => 
            c.UserId == userId && 
            c.PostId == postId &&
            c.ParentCommentId == parentCommentId &&
            c.IsApproved == true));
    }

    [Fact]
    public async Task ReplyToCommentAsync_WithNonExistentParentComment_ShouldThrowApplicationException()
    {
        // Arrange
        var parentCommentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CreateCommentRequest { Content = "Reply content" };

        _commentRepository.GetByIdAsync(parentCommentId).Returns((Comment)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _commentService.ReplyToCommentAsync(parentCommentId, userId, request)
        );

        exception.Message.Should().Be("Parent comment not found");
    }

    #endregion
}