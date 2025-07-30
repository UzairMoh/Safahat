using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Safahat.Application.DTOs.Requests.Users;
using Safahat.Application.DTOs.Responses.Users;
using Safahat.Application.Services;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Tests.Services;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _commentRepository = Substitute.For<ICommentRepository>();
        _mapper = Substitute.For<IMapper>();
        
        _userService = new UserService(
            _userRepository,
            _postRepository,
            _commentRepository,
            _mapper
        );
    }

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnMappedUserList()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "user1", Email = "user1@example.com" },
            new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@example.com" }
        };

        var expectedResponse = new List<UserListItemResponse>
        {
            new UserListItemResponse { Id = users[0].Id, Username = "user1", Email = "user1@example.com" },
            new UserListItemResponse { Id = users[1].Id, Username = "user2", Email = "user2@example.com" }
        };

        _userRepository.GetAllAsync().Returns(users);
        _mapper.Map<IEnumerable<UserListItemResponse>>(users).Returns(expectedResponse);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _userRepository.Received(1).GetAllAsync();
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUserDetailWithCounts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var posts = new List<Post>
        {
            new Post { Id = Guid.NewGuid(), AuthorId = userId },
            new Post { Id = Guid.NewGuid(), AuthorId = userId }
        };

        var comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), UserId = userId },
            new Comment { Id = Guid.NewGuid(), UserId = userId },
            new Comment { Id = Guid.NewGuid(), UserId = userId }
        };

        var expectedUserDetail = new UserDetailResponse
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetPostsByAuthorAsync(userId).Returns(posts);
        _commentRepository.GetCommentsByUserAsync(userId).Returns(comments);
        _mapper.Map<UserDetailResponse>(user).Returns(expectedUserDetail);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Username.Should().Be("testuser");
        result.PostCount.Should().Be(2);
        result.CommentCount.Should().Be(3);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _userService.GetUserByIdAsync(userId)
        );

        exception.Message.Should().Be("User not found");
        await _postRepository.DidNotReceive().GetPostsByAuthorAsync(Arg.Any<Guid>());
        await _commentRepository.DidNotReceive().GetCommentsByUserAsync(Arg.Any<Guid>());
    }

    #endregion

    #region GetUserByUsernameAsync Tests

    [Fact]
    public async Task GetUserByUsernameAsync_WithExistingUser_ShouldReturnUserDetailWithCounts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var user = new User
        {
            Id = userId,
            Username = username,
            Email = "test@example.com"
        };

        var posts = new List<Post>
        {
            new Post { Id = Guid.NewGuid(), AuthorId = userId }
        };

        var comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), UserId = userId },
            new Comment { Id = Guid.NewGuid(), UserId = userId }
        };

        var expectedUserDetail = new UserDetailResponse
        {
            Id = userId,
            Username = username,
            Email = "test@example.com"
        };

        _userRepository.GetByUsernameAsync(username).Returns(user);
        _postRepository.GetPostsByAuthorAsync(userId).Returns(posts);
        _commentRepository.GetCommentsByUserAsync(userId).Returns(comments);
        _mapper.Map<UserDetailResponse>(user).Returns(expectedUserDetail);

        // Act
        var result = await _userService.GetUserByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be(username);
        result.PostCount.Should().Be(1);
        result.CommentCount.Should().Be(2);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var username = "nonexistentuser";
        _userRepository.GetByUsernameAsync(username).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _userService.GetUserByUsernameAsync(username)
        );

        exception.Message.Should().Be("User not found");
    }

    #endregion

    #region UpdateUserRoleAsync Tests

    [Fact]
    public async Task UpdateUserRoleAsync_WithExistingUser_ShouldUpdateRoleAndReturnUserDetail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Role = UserRole.Reader
        };

        var updateRequest = new UpdateUserRoleRequest
        {
            Role = UserRole.Author
        };

        var expectedUserDetail = new UserDetailResponse
        {
            Id = userId,
            Username = "testuser",
            Role = UserRole.Author
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetPostsByAuthorAsync(userId).Returns(new List<Post>());
        _commentRepository.GetCommentsByUserAsync(userId).Returns(new List<Comment>());
        _mapper.Map<UserDetailResponse>(Arg.Any<User>()).Returns(expectedUserDetail);

        // Act
        var result = await _userService.UpdateUserRoleAsync(userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(UserRole.Author);
        
        await _userRepository.Received(1).UpdateAsync(Arg.Is<User>(u => 
            u.Id == userId && 
            u.Role == UserRole.Author &&
            u.UpdatedAt != null));
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserRoleRequest { Role = UserRole.Author };

        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _userService.UpdateUserRoleAsync(userId, updateRequest)
        );

        exception.Message.Should().Be("User not found");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    #endregion

    #region UpdateUserStatusAsync Tests

    [Fact]
    public async Task UpdateUserStatusAsync_WithExistingUser_ShouldUpdateStatusAndReturnUserDetail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            IsActive = true
        };

        var updateRequest = new UpdateUserStatusRequest
        {
            IsActive = false
        };

        var expectedUserDetail = new UserDetailResponse
        {
            Id = userId,
            Username = "testuser",
            IsActive = false
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetPostsByAuthorAsync(userId).Returns(new List<Post>());
        _commentRepository.GetCommentsByUserAsync(userId).Returns(new List<Comment>());
        _mapper.Map<UserDetailResponse>(Arg.Any<User>()).Returns(expectedUserDetail);

        // Act
        var result = await _userService.UpdateUserStatusAsync(userId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
        
        await _userRepository.Received(1).UpdateAsync(Arg.Is<User>(u => 
            u.Id == userId && 
            u.IsActive == false &&
            u.UpdatedAt != null));
    }

    [Fact]
    public async Task UpdateUserStatusAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserStatusRequest { IsActive = false };

        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _userService.UpdateUserStatusAsync(userId, updateRequest)
        );

        exception.Message.Should().Be("User not found");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_WithExistingUser_ShouldSoftDeleteAndReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            Bio = "Test bio",
            ProfilePictureUrl = "http://example.com/pic.jpg"
        };

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        
        await _userRepository.Received(1).UpdateAsync(Arg.Is<User>(u => 
            u.Id == userId && 
            u.IsActive == false &&
            u.Email == $"deleted_{userId}@example.com" &&
            u.Username == $"deleted_user_{userId}" &&
            u.FirstName == "Deleted" &&
            u.LastName == "User" &&
            u.Bio == null &&
            u.ProfilePictureUrl == null &&
            u.UpdatedAt != null));
    }

    [Fact]
    public async Task DeleteUserAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _userService.DeleteUserAsync(userId)
        );

        exception.Message.Should().Be("User not found");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    #endregion

    #region GetUserStatisticsAsync Tests

    [Fact]
    public async Task GetUserStatisticsAsync_WithExistingUser_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser" };

        var posts = new List<Post>
        {
            new Post { Id = Guid.NewGuid(), AuthorId = userId, Status = PostStatus.Published },
            new Post { Id = Guid.NewGuid(), AuthorId = userId, Status = PostStatus.Published },
            new Post { Id = Guid.NewGuid(), AuthorId = userId, Status = PostStatus.Draft }
        };

        var comments = new List<Comment>
        {
            new Comment { Id = Guid.NewGuid(), UserId = userId },
            new Comment { Id = Guid.NewGuid(), UserId = userId },
            new Comment { Id = Guid.NewGuid(), UserId = userId },
            new Comment { Id = Guid.NewGuid(), UserId = userId }
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetPostsByAuthorAsync(userId).Returns(posts);
        _commentRepository.GetCommentsByUserAsync(userId).Returns(comments);

        // Act
        var result = await _userService.GetUserStatisticsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalPosts.Should().Be(3);
        result.PublishedPosts.Should().Be(2);
        result.DraftPosts.Should().Be(1);
        result.TotalComments.Should().Be(4);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _userService.GetUserStatisticsAsync(userId)
        );

        exception.Message.Should().Be("User not found");
        await _postRepository.DidNotReceive().GetPostsByAuthorAsync(Arg.Any<Guid>());
        await _commentRepository.DidNotReceive().GetCommentsByUserAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task GetUserStatisticsAsync_WithUserHavingNoPostsOrComments_ShouldReturnZeroStatistics()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser" };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _postRepository.GetPostsByAuthorAsync(userId).Returns(new List<Post>());
        _commentRepository.GetCommentsByUserAsync(userId).Returns(new List<Comment>());

        // Act
        var result = await _userService.GetUserStatisticsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.TotalPosts.Should().Be(0);
        result.PublishedPosts.Should().Be(0);
        result.DraftPosts.Should().Be(0);
        result.TotalComments.Should().Be(0);
    }

    #endregion
}