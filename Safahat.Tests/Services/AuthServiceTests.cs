using System.Text;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Safahat.Application.DTOs.Requests;
using Safahat.Application.DTOs.Responses.Auth;
using Safahat.Application.Services;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Tests.Services;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _mapper = Substitute.For<IMapper>();
        _configuration = Substitute.For<IConfiguration>();
        
        // Setup configuration for JWT
        _configuration["Jwt:Key"].Returns("ThisIsASecretKeyForTestingPurposesOnlyAndNeedsToBeAtLeast32Chars");
        _configuration["Jwt:Issuer"].Returns("TestIssuer");
        _configuration["Jwt:Audience"].Returns("TestAudience");
        
        _authService = new AuthService(
            _userRepository,
            _mapper,
            _configuration
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = loginRequest.Email,
            Username = "testuser",
            PasswordHash = _authService.HashPassword("Password123!"),
            IsActive = true,
            Role = UserRole.Reader
        };

        var expectedUserResponse = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = UserRole.Reader
        };

        _userRepository.GetByEmailAsync(loginRequest.Email).Returns(user);
        _mapper.Map<UserResponse>(user).Returns(expectedUserResponse);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().BeEquivalentTo(expectedUserResponse);
        result.Token.Should().NotBeNullOrEmpty();
        result.Expiration.Should().BeAfter(DateTime.UtcNow.AddDays(6)); // Token should be valid for at least 6 days
        
        await _userRepository.Received(1).UpdateAsync(Arg.Is<User>(u => 
            u.Id == user.Id && 
            u.LastLoginAt != null));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowApplicationException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _userRepository.GetByEmailAsync(loginRequest.Email).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.LoginAsync(loginRequest)
        );

        exception.Message.Should().Be("Invalid email or password");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowApplicationException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = loginRequest.Email,
            Username = "testuser",
            PasswordHash = _authService.HashPassword("Password123!"),
            IsActive = true
        };

        _userRepository.GetByEmailAsync(loginRequest.Email).Returns(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.LoginAsync(loginRequest)
        );

        exception.Message.Should().Be("Invalid email or password");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task LoginAsync_WithInactiveAccount_ShouldThrowApplicationException()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = loginRequest.Email,
            Username = "inactiveuser",
            PasswordHash = _authService.HashPassword("Password123!"),
            IsActive = false
        };

        _userRepository.GetByEmailAsync(loginRequest.Email).Returns(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.LoginAsync(loginRequest)
        );

        exception.Message.Should().Be("Account is inactive");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserAndReturnAuthResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Username = "newuser",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            Email = registerRequest.Email,
            Username = registerRequest.Username,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName,
            Role = UserRole.Reader,
            IsActive = true
        };

        var expectedUserResponse = new UserResponse
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            Username = createdUser.Username,
            FirstName = createdUser.FirstName,
            LastName = createdUser.LastName,
            Role = UserRole.Reader
        };

        _userRepository.GetByEmailAsync(registerRequest.Email).Returns((User)null);
        _userRepository.GetByUsernameAsync(registerRequest.Username).Returns((User)null);
        _mapper.Map<User>(registerRequest).Returns(createdUser);
        _userRepository.AddAsync(Arg.Any<User>()).Returns(createdUser);
        _mapper.Map<UserResponse>(createdUser).Returns(expectedUserResponse);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().BeEquivalentTo(expectedUserResponse);
        result.Token.Should().NotBeNullOrEmpty();
        result.Expiration.Should().BeAfter(DateTime.UtcNow.AddDays(6));
        
        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u => 
            u.Email == registerRequest.Email && 
            u.Username == registerRequest.Username &&
            u.Role == UserRole.Reader &&
            u.PasswordHash != null));
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowApplicationException()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "existing@example.com",
            Username = "newuser",
            Password = "Password123!"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = registerRequest.Email,
            Username = "existinguser"
        };

        _userRepository.GetByEmailAsync(registerRequest.Email).Returns(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.RegisterAsync(registerRequest)
        );

        exception.Message.Should().Be("Email is already registered");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldThrowApplicationException()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Username = "existinguser",
            Password = "Password123!"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            Username = registerRequest.Username
        };

        _userRepository.GetByEmailAsync(registerRequest.Email).Returns((User)null);
        _userRepository.GetByUsernameAsync(registerRequest.Username).Returns(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.RegisterAsync(registerRequest)
        );

        exception.Message.Should().Be("Username is already taken");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ShouldUpdatePasswordAndReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword456!"
        };

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Username = "username",
            PasswordHash = _authService.HashPassword("CurrentPassword123!")
        };

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _authService.ChangePasswordAsync(userId, changePasswordRequest);

        // Assert
        result.Should().BeTrue();
        await _userRepository.Received(1).UpdateAsync(Arg.Is<User>(u => 
            u.Id == userId && 
            u.PasswordHash != _authService.HashPassword("CurrentPassword123!") &&
            u.UpdatedAt != null));
    }

    [Fact]
    public async Task ChangePasswordAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword456!"
        };

        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.ChangePasswordAsync(userId, changePasswordRequest)
        );

        exception.Message.Should().Be("User not found");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var changePasswordRequest = new ChangePasswordRequest
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "NewPassword456!"
        };

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Username = "username",
            PasswordHash = _authService.HashPassword("CurrentPassword123!")
        };

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.ChangePasswordAsync(userId, changePasswordRequest)
        );

        exception.Message.Should().Be("Current password is incorrect");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task GetUserProfileAsync_WithExistingUser_ShouldReturnUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Username = "username",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Reader
        };

        var expectedUserResponse = new UserResponse
        {
            Id = userId,
            Email = "user@example.com",
            Username = "username",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Reader
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _mapper.Map<UserResponse>(user).Returns(expectedUserResponse);

        // Act
        var result = await _authService.GetUserProfileAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedUserResponse);
    }

    [Fact]
    public async Task GetUserProfileAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.GetUserProfileAsync(userId)
        );

        exception.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithValidData_ShouldUpdateProfileAndReturnUserResponse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateProfileRequest = new UpdateUserProfileRequest
        {
            FirstName = "Updated",
            LastName = "Name",
            Bio = "Updated bio"
        };

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            Username = "username",
            FirstName = "Original",
            LastName = "Name",
            Bio = "Original bio"
        };

        var updatedUser = new User
        {
            Id = userId,
            Email = "user@example.com",
            Username = "username",
            FirstName = "Updated",
            LastName = "Name",
            Bio = "Updated bio"
        };

        var expectedUserResponse = new UserResponse
        {
            Id = userId,
            Email = "user@example.com",
            Username = "username",
            FirstName = "Updated",
            LastName = "Name",
            Bio = "Updated bio"
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _mapper.Map<UserResponse>(Arg.Any<User>()).Returns(expectedUserResponse);

        // Act
        var result = await _authService.UpdateUserProfileAsync(userId, updateProfileRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedUserResponse);
        
        await _userRepository.Received(1).UpdateAsync(Arg.Is<User>(u => 
            u.Id == userId && 
            u.UpdatedAt != null));
    }

    [Fact]
    public async Task UpdateUserProfileAsync_WithNonExistentUser_ShouldThrowApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateProfileRequest = new UpdateUserProfileRequest
        {
            FirstName = "Updated",
            LastName = "Name"
        };

        _userRepository.GetByIdAsync(userId).Returns((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _authService.UpdateUserProfileAsync(userId, updateProfileRequest)
        );

        exception.Message.Should().Be("User not found");
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }
}