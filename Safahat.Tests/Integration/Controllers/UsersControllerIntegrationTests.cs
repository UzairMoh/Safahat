using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Safahat.Application.DTOs.Requests.Users;
using Safahat.Application.DTOs.Responses.Users;
using Safahat.Models.Enums;
using Safahat.Tests.Integration.Infrastructure;

namespace Safahat.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for UsersController covering user management functionality
/// Keeps tests simple and focused on core scenarios
/// </summary>
public class UsersControllerIntegrationTests : IClassFixture<SafahatWebApplicationFactory>
{
    private readonly SafahatWebApplicationFactory _factory;
    private readonly HttpClient _unauthenticatedClient;

    public UsersControllerIntegrationTests(SafahatWebApplicationFactory factory)
    {
        _factory = factory;
        _unauthenticatedClient = _factory.CreateUnauthenticatedClient();
    }

    #region Admin-Only Endpoints

    [Fact]
    public async Task GetAllUsers_AsAdmin_ShouldReturnAllUsers()
    {
        // Arrange
        var client = _factory.CreateAdminClient();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<UserListItemResponse[]>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
        users.Should().Contain(u => u.Id == TestDataSeeder.ReaderUserId);
        users.Should().Contain(u => u.Id == TestDataSeeder.AuthorUserId);
        users.Should().Contain(u => u.Id == TestDataSeeder.AdminUserId);
    }

    [Fact]
    public async Task GetAllUsers_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsers_AsUnauthenticated_ShouldReturn401()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Get User Details

    [Fact]
    public async Task GetUserById_AsOwner_ShouldReturnUserDetails()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync($"/api/users/{TestDataSeeder.ReaderUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDetailResponse>();
        user.Should().NotBeNull();
        user.Id.Should().Be(TestDataSeeder.ReaderUserId);
        user.Username.Should().Be("readeruser");
        user.Role.Should().Be(UserRole.Reader);
        user.PostCount.Should().BeGreaterThanOrEqualTo(0);
        user.CommentCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetUserById_AsAdmin_ShouldReturnAnyUserDetails()
    {
        // Arrange
        var client = _factory.CreateAdminClient();

        // Act
        var response = await client.GetAsync($"/api/users/{TestDataSeeder.ReaderUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDetailResponse>();
        user.Should().NotBeNull();
        user.Id.Should().Be(TestDataSeeder.ReaderUserId);
    }

    [Fact]
    public async Task GetUserById_AsOtherUser_ShouldReturn403()
    {
        // Arrange - ReaderUserId trying to access OtherReaderUserId
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync($"/api/users/{TestDataSeeder.OtherReaderUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_WithNonExistentId_ShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/users/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserByUsername_AsOwner_ShouldReturnUserDetails()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync("/api/users/username/readeruser");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDetailResponse>();
        user.Should().NotBeNull();
        user.Username.Should().Be("readeruser");
        user.Id.Should().Be(TestDataSeeder.ReaderUserId);
    }

    [Fact]
    public async Task GetUserByUsername_AsOtherUser_ShouldReturn403()
    {
        // Arrange - ReaderUserId trying to access otherreader
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync("/api/users/username/otherreader");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Admin User Management

    [Fact]
    public async Task UpdateUserRole_AsAdmin_ShouldUpdateRole()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var request = new UpdateUserRoleRequest
        {
            Role = UserRole.Author
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{TestDataSeeder.ReaderUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedUser = await response.Content.ReadFromJsonAsync<UserDetailResponse>();
        updatedUser.Should().NotBeNull();
        updatedUser.Role.Should().Be(UserRole.Author);
        updatedUser.Id.Should().Be(TestDataSeeder.ReaderUserId);
    }

    [Fact]
    public async Task UpdateUserRole_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();
        var request = new UpdateUserRoleRequest
        {
            Role = UserRole.Admin
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{TestDataSeeder.OtherReaderUserId}/role", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUserStatus_AsAdmin_ShouldUpdateStatus()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var request = new UpdateUserStatusRequest
        {
            IsActive = false
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{TestDataSeeder.ReaderUserId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedUser = await response.Content.ReadFromJsonAsync<UserDetailResponse>();
        updatedUser.Should().NotBeNull();
        updatedUser.IsActive.Should().BeFalse();
        updatedUser.Id.Should().Be(TestDataSeeder.ReaderUserId);
    }

    [Fact]
    public async Task UpdateUserStatus_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();
        var request = new UpdateUserStatusRequest
        {
            IsActive = false
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/users/{TestDataSeeder.OtherReaderUserId}/status", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_AsAdmin_ShouldDeleteUser()
    {
        // Arrange
        var client = _factory.CreateAdminClient();

        // Act
        var response = await client.DeleteAsync($"/api/users/{TestDataSeeder.InactiveUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is "deleted" (soft delete - should still exist but be inactive)
        var getResponse = await client.GetAsync($"/api/users/{TestDataSeeder.InactiveUserId}");
        if (getResponse.StatusCode == HttpStatusCode.OK)
        {
            var user = await getResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
            user.IsActive.Should().BeFalse(); // Should be deactivated
        }
    }

    [Fact]
    public async Task DeleteUser_OwnAccount_ShouldReturn400()
    {
        // Arrange
        var client = _factory.CreateAdminClient();

        // Act - Admin trying to delete their own account
        var response = await client.DeleteAsync($"/api/users/{TestDataSeeder.AdminUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteUser_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.DeleteAsync($"/api/users/{TestDataSeeder.OtherReaderUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region User Statistics

    [Fact]
    public async Task GetUserStatistics_AsOwner_ShouldReturnStatistics()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync($"/api/users/{TestDataSeeder.ReaderUserId}/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statistics = await response.Content.ReadFromJsonAsync<UserStatisticsResponse>();
        statistics.Should().NotBeNull();
        statistics.TotalPosts.Should().BeGreaterThanOrEqualTo(0);
        statistics.PublishedPosts.Should().BeGreaterThanOrEqualTo(0);
        statistics.DraftPosts.Should().BeGreaterThanOrEqualTo(0);
        statistics.TotalComments.Should().BeGreaterThanOrEqualTo(0);
        
        // Logical validation
        statistics.TotalPosts.Should().Be(statistics.PublishedPosts + statistics.DraftPosts);
    }

    [Fact]
    public async Task GetUserStatistics_AsAdmin_ShouldReturnAnyUserStatistics()
    {
        // Arrange
        var client = _factory.CreateAdminClient();

        // Act
        var response = await client.GetAsync($"/api/users/{TestDataSeeder.ReaderUserId}/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statistics = await response.Content.ReadFromJsonAsync<UserStatisticsResponse>();
        statistics.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserStatistics_AsOtherUser_ShouldReturn403()
    {
        // Arrange - ReaderUserId trying to access OtherReaderUserId's stats
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync($"/api/users/{TestDataSeeder.OtherReaderUserId}/statistics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}