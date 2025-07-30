using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Posts;
using Safahat.Tests.Integration.Infrastructure;

namespace Safahat.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for PostsController covering all HTTP endpoints
/// Tests the complete request pipeline including authentication, authorisation, and data persistence
/// </summary>
public class PostsControllerIntegrationTests : IClassFixture<SafahatWebApplicationFactory>
{
    private readonly SafahatWebApplicationFactory _factory;
    private readonly HttpClient _unauthenticatedClient;

    public PostsControllerIntegrationTests(SafahatWebApplicationFactory factory)
    {
        _factory = factory;
        _unauthenticatedClient = _factory.CreateUnauthenticatedClient();
    }

    #region Public Endpoints (No Authentication Required)

    [Fact]
    public async Task GetPublishedPosts_ShouldReturnOnlyPublishedPosts()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/posts/published");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().OnlyContain(p => p.Status == Models.Enums.PostStatus.Published);
        posts.Should().Contain(p => p.Id == TestDataSeeder.PublishedPostId);
        posts.Should().NotContain(p => p.Id == TestDataSeeder.DraftPostId);
    }

    [Fact]
    public async Task GetPublishedPosts_WithPagination_ShouldReturnCorrectPage()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/posts/published?pageNumber=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().HaveCountLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetPostById_WhenPostExists_ShouldReturnPost()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync($"/api/posts/{TestDataSeeder.PublishedPostId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        post.Should().NotBeNull();
        post.Id.Should().Be(TestDataSeeder.PublishedPostId);
        post.Title.Should().Be("Getting Started with C# Testing");
    }

    [Fact]
    public async Task GetPostById_WhenPostDoesNotExist_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _unauthenticatedClient.GetAsync($"/api/posts/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPostBySlug_WhenPostExists_ShouldReturnPostAndIncrementViewCount()
    {
        // Arrange
        var slug = "getting-started-csharp-testing";

        // Get initial view count
        var initialResponse = await _unauthenticatedClient.GetAsync($"/api/posts/{TestDataSeeder.PublishedPostId}");
        var initialPost = await initialResponse.Content.ReadFromJsonAsync<PostResponse>();
        var initialViewCount = initialPost.ViewCount;

        // Act
        var response = await _unauthenticatedClient.GetAsync($"/api/posts/slug/{slug}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        post.Should().NotBeNull();
        post.Id.Should().Be(TestDataSeeder.PublishedPostId);
        post.ViewCount.Should().Be(initialViewCount + 1);
    }

    [Fact]
    public async Task GetPostBySlug_WhenPostDoesNotExist_ShouldReturn404()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/posts/slug/non-existent-slug");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchPosts_WithValidQuery_ShouldReturnMatchingPosts()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/posts/search?query=testing");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().OnlyContain(p => p.Status == Models.Enums.PostStatus.Published);
        posts.Should().Contain(p => p.Title.Contains("Testing") || p.Content.Contains("testing"));
    }

    [Fact]
    public async Task SearchPosts_WithEmptyQuery_ShouldReturn400()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/posts/search?query=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPostsByCategory_ShouldReturnPostsFromCategory()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync($"/api/posts/category/{TestDataSeeder.TechnologyCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().OnlyContain(p => p.Status == Models.Enums.PostStatus.Published);
        posts.Should().Contain(p => p.Id == TestDataSeeder.PublishedPostId);
    }

    [Fact]
    public async Task GetFeaturedPosts_ShouldReturnOnlyFeaturedPosts()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/posts/featured");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().OnlyContain(p => p.IsFeatured == true && p.Status == Models.Enums.PostStatus.Published);
        posts.Should().Contain(p => p.Id == TestDataSeeder.FeaturedPostId);
    }

    #endregion

    #region Authenticated User Endpoints

    [Fact]
    public async Task CreatePost_AsAuthenticatedUser_ShouldCreatePost()
    {
        // Arrange
        var client = _factory.CreateReaderClient();
        var request = new CreatePostRequest
        {
            Title = "New Integration Test Post",
            Content = "This is content for integration testing",
            Summary = "Integration test summary",
            IsDraft = false,
            CategoryIds = new List<Guid> { TestDataSeeder.TechnologyCategoryId },
            Tags = new List<string> { "integration", "testing" }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/posts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        post.Should().NotBeNull();
        post.Title.Should().Be(request.Title);
        post.Content.Should().Be(request.Content);
        post.Status.Should().Be(Models.Enums.PostStatus.Published);
        post.Author.Id.Should().Be(TestDataSeeder.ReaderUserId);
    }

    [Fact]
    public async Task CreatePost_AsUnauthenticatedUser_ShouldReturn401()
    {
        // Arrange
        var request = new CreatePostRequest
        {
            Title = "Unauthorized Post",
            Content = "This should fail",
            Summary = "Should not work"
        };

        // Act
        var response = await _unauthenticatedClient.PostAsJsonAsync("/api/posts", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePost_AsPostAuthor_ShouldUpdatePost()
    {
        // Arrange
        var client = _factory.CreateReaderClient(); // ReaderUserId owns PublishedPostId
        var request = new UpdatePostRequest
        {
            Title = "Updated Test Post Title",
            Content = "Updated content for testing",
            Summary = "Updated summary"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/posts/{TestDataSeeder.PublishedPostId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        post.Should().NotBeNull();
        post.Title.Should().Be(request.Title);
        post.Content.Should().Be(request.Content);
    }

    [Fact]
    public async Task UpdatePost_AsOtherUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateOtherReaderClient(); // Different user than post owner
        var request = new UpdatePostRequest
        {
            Title = "Unauthorized Update",
            Content = "This should not work"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/posts/{TestDataSeeder.PublishedPostId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdatePost_AsAdmin_ShouldUpdateAnyPost()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var request = new UpdatePostRequest
        {
            Title = "Admin Updated Post",
            Content = "Admin can edit any post"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/posts/{TestDataSeeder.PublishedPostId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var post = await response.Content.ReadFromJsonAsync<PostResponse>();
        post.Title.Should().Be(request.Title);
    }

    [Fact]
    public async Task DeletePost_AsPostAuthor_ShouldDeletePost()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.DeleteAsync($"/api/posts/{TestDataSeeder.DraftPostId}"); // ReaderUserId owns this

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify post is deleted
        var getResponse = await _unauthenticatedClient.GetAsync($"/api/posts/{TestDataSeeder.DraftPostId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PublishPost_AsPostAuthor_ShouldPublishPost()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.PutAsync($"/api/posts/{TestDataSeeder.DraftPostId}/publish", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify post is published
        var getResponse = await _unauthenticatedClient.GetAsync($"/api/posts/{TestDataSeeder.DraftPostId}");
        var post = await getResponse.Content.ReadFromJsonAsync<PostResponse>();
        post.Status.Should().Be(Models.Enums.PostStatus.Published);
        post.PublishedAt.Should().NotBeNull();
    }

    #endregion

    #region Admin-Only Endpoints

    [Fact]
    public async Task GetAllPosts_AsAdmin_ShouldReturnAllPostsIncludingDrafts()
    {
        // Arrange
        var client = _factory.CreateAdminClient();

        // Act
        var response = await client.GetAsync("/api/posts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().Contain(p => p.Id == TestDataSeeder.PublishedPostId);
        posts.Should().Contain(p => p.Id == TestDataSeeder.DraftPostId);
        posts.Should().Contain(p => p.Status == Models.Enums.PostStatus.Draft);
    }

    [Fact]
    public async Task GetAllPosts_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync("/api/posts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task FeaturePost_AsAdmin_ShouldFeaturePost()
    {
        // Arrange
        var client = _factory.CreateAdminClient();

        // Act
        var response = await client.PutAsync($"/api/posts/{TestDataSeeder.PublishedPostId}/feature", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify post is featured
        var getResponse = await _unauthenticatedClient.GetAsync($"/api/posts/{TestDataSeeder.PublishedPostId}");
        var post = await getResponse.Content.ReadFromJsonAsync<PostResponse>();
        post.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public async Task FeaturePost_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.PutAsync($"/api/posts/{TestDataSeeder.PublishedPostId}/feature", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Authorisation Tests

    [Fact]
    public async Task GetPostsByAuthor_AsOwner_ShouldReturnAllOwnPosts()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.GetAsync($"/api/posts/author/{TestDataSeeder.ReaderUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().Contain(p => p.Status == Models.Enums.PostStatus.Published);
        posts.Should().Contain(p => p.Status == Models.Enums.PostStatus.Draft); // Owner sees drafts
    }

    [Fact]
    public async Task GetPostsByAuthor_AsOtherUser_ShouldReturnOnlyPublishedPosts()
    {
        // Arrange - viewing ReaderUserId's posts as OtherReaderUserId

        // Act
        var response = await _unauthenticatedClient.GetAsync($"/api/posts/author/{TestDataSeeder.ReaderUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().OnlyContain(p => p.Status == Models.Enums.PostStatus.Published); // Others only see published
    }

    #endregion
}