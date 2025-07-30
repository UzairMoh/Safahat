using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Safahat.Application.DTOs.Requests.Categories;
using Safahat.Application.DTOs.Responses.Categories;
using Safahat.Application.DTOs.Responses.Posts;
using Safahat.Tests.Integration.Infrastructure;

namespace Safahat.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for CategoriesController covering all HTTP endpoints
/// Tests the complete request pipeline including authentication, authorisation, and category-post relationships
/// </summary>
public class CategoriesControllerIntegrationTests : IClassFixture<SafahatWebApplicationFactory>
{
    private readonly SafahatWebApplicationFactory _factory;
    private readonly HttpClient _unauthenticatedClient;

    public CategoriesControllerIntegrationTests(SafahatWebApplicationFactory factory)
    {
        _factory = factory;
        _unauthenticatedClient = _factory.CreateUnauthenticatedClient();
    }

    #region Public Endpoints (No Authentication Required)

    [Fact]
    public async Task GetAllCategories_ShouldReturnAllCategories()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<CategoryResponse[]>();
        categories.Should().NotBeNull();
        categories.Should().NotBeEmpty();
        categories.Should().Contain(c => c.Id == TestDataSeeder.TechnologyCategoryId);
        categories.Should().Contain(c => c.Id == TestDataSeeder.LifestyleCategoryId);
    }

    [Fact]
    public async Task GetCategoriesWithPostCount_ShouldIncludePostCounts()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/categories/with-post-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<CategoryResponse[]>();
        categories.Should().NotBeNull();
        categories.Should().NotBeEmpty();
        
        // Technology category should have posts
        var techCategory = categories.Should().Contain(c => c.Id == TestDataSeeder.TechnologyCategoryId).Subject;
        techCategory.PostCount.Should().BeGreaterThan(0);
        
        // All categories should have PostCount property set (even if 0)
        categories.Should().OnlyContain(c => c.PostCount >= 0);
    }

    [Fact]
    public async Task GetCategoryById_WhenExists_ShouldReturnCategory()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync($"/api/categories/{TestDataSeeder.TechnologyCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        category.Should().NotBeNull();
        category.Id.Should().Be(TestDataSeeder.TechnologyCategoryId);
        category.Name.Should().Be("Technology");
        category.Slug.Should().Be("technology");
        category.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCategoryById_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _unauthenticatedClient.GetAsync($"/api/categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCategoryBySlug_WhenExists_ShouldReturnCategory()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/categories/slug/technology");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        category.Should().NotBeNull();
        category.Id.Should().Be(TestDataSeeder.TechnologyCategoryId);
        category.Name.Should().Be("Technology");
        category.Slug.Should().Be("technology");
    }

    [Fact]
    public async Task GetCategoryBySlug_WhenNotExists_ShouldReturn404()
    {
        // Act
        var response = await _unauthenticatedClient.GetAsync("/api/categories/slug/non-existent-category");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Admin-Only Endpoints

    [Fact]
    public async Task CreateCategory_AsAdmin_ShouldCreateCategory()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var request = new CreateCategoryRequest
        {
            Name = "Science",
            Slug = "science",
            Description = "Science and research related posts"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        category.Should().NotBeNull();
        category.Name.Should().Be(request.Name);
        category.Slug.Should().Be(request.Slug);
        category.Description.Should().Be(request.Description);
        category.PostCount.Should().Be(0); // New category should have 0 posts
    }

    [Fact]
    public async Task CreateCategory_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();
        var request = new CreateCategoryRequest
        {
            Name = "Unauthorised Category",
            Slug = "unauthorised",
            Description = "This should not work"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCategory_AsUnauthenticated_ShouldReturn401()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Unauthenticated Category",
            Slug = "unauthenticated",
            Description = "This should fail"
        };

        // Act
        var response = await _unauthenticatedClient.PostAsJsonAsync("/api/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateSlug_ShouldReturn400()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var request = new CreateCategoryRequest
        {
            Name = "Duplicate Technology",
            Slug = "technology", // This slug already exists
            Description = "Duplicate slug test"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_WithEmptySlug_ShouldAutoGenerateSlug()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var request = new CreateCategoryRequest
        {
            Name = "Auto Generated Slug Category",
            Slug = "", // Empty slug should trigger auto-generation
            Description = "Testing auto slug generation"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        category.Should().NotBeNull();
        category.Slug.Should().NotBeNullOrEmpty();
        category.Slug.Should().Be("auto-generated-slug-category"); // Expected auto-generated slug
    }

    [Fact]
    public async Task UpdateCategory_AsAdmin_ShouldUpdateCategory()
    {
        // Arrange - First create a category to update
        _factory.ResetDatabase();
        var adminClient = _factory.CreateClientWithoutReset(
            TestDataSeeder.AdminUserId,
            "Admin",
            "adminuser",
            "admin@test.com"
        );

        var createRequest = new CreateCategoryRequest
        {
            Name = "Original Category",
            Slug = "original-category",
            Description = "Original description"
        };

        var createResponse = await adminClient.PostAsJsonAsync("/api/categories", createRequest);
        var createdCategory = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        var updateRequest = new UpdateCategoryRequest
        {
            Name = "Updated Category",
            Slug = "updated-category",
            Description = "Updated description"
        };

        // Act
        var response = await adminClient.PutAsJsonAsync($"/api/categories/{createdCategory.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedCategory = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        updatedCategory.Should().NotBeNull();
        updatedCategory.Name.Should().Be(updateRequest.Name);
        updatedCategory.Slug.Should().Be(updateRequest.Slug);
        updatedCategory.Description.Should().Be(updateRequest.Description);
        updatedCategory.Id.Should().Be(createdCategory.Id); // ID should remain the same
    }

    [Fact]
    public async Task UpdateCategory_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();
        var request = new UpdateCategoryRequest
        {
            Name = "Unauthorised Update",
            Slug = "unauthorised-update",
            Description = "This should not work"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/categories/{TestDataSeeder.TechnologyCategoryId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCategory_WithNonExistentId_ShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateCategoryRequest
        {
            Name = "Non-existent Category",
            Slug = "non-existent",
            Description = "This category doesn't exist"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/categories/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_AsAdmin_ShouldDeleteCategory()
    {
        // Arrange - First create a category to delete
        _factory.ResetDatabase();
        var adminClient = _factory.CreateClientWithoutReset(
            TestDataSeeder.AdminUserId,
            "Admin",
            "adminuser",
            "admin@test.com"
        );

        var createRequest = new CreateCategoryRequest
        {
            Name = "Category to Delete",
            Slug = "category-to-delete",
            Description = "This category will be deleted"
        };

        var createResponse = await adminClient.PostAsJsonAsync("/api/categories", createRequest);
        var createdCategory = await createResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        // Act
        var response = await adminClient.DeleteAsync($"/api/categories/{createdCategory.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify category is deleted
        var publicClient = _factory.CreateClientWithoutReset(Guid.NewGuid(), "Public", "public", "public@test.com");
        var getResponse = await publicClient.GetAsync($"/api/categories/{createdCategory.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCategory_AsRegularUser_ShouldReturn403()
    {
        // Arrange
        var client = _factory.CreateReaderClient();

        // Act
        var response = await client.DeleteAsync($"/api/categories/{TestDataSeeder.LifestyleCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCategory_WithNonExistentId_ShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateAdminClient();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await client.DeleteAsync($"/api/categories/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Category-Post Relationships

    [Fact]
    public async Task GetPostsByCategory_ShouldReturnCategoryPosts()
    {
        // Act - Get posts in Technology category
        var response = await _unauthenticatedClient.GetAsync($"/api/posts/category/{TestDataSeeder.TechnologyCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().NotBeEmpty();
        
        // All posts should be published (public endpoint)
        posts.Should().OnlyContain(p => p.Status == Models.Enums.PostStatus.Published);
        
        // Should contain posts that belong to Technology category
        posts.Should().Contain(p => p.Id == TestDataSeeder.PublishedPostId);
        
        // All posts should have the Technology category
        posts.Should().OnlyContain(p => p.Categories.Any(c => c.Id == TestDataSeeder.TechnologyCategoryId));
    }

    [Fact]
    public async Task CategoryPostCount_ShouldReflectActualPostCount()
    {
        // Arrange - Get the category with post count
        var categoriesResponse = await _unauthenticatedClient.GetAsync("/api/categories/with-post-count");
        var categories = await categoriesResponse.Content.ReadFromJsonAsync<CategoryResponse[]>();
        var techCategory = categories.First(c => c.Id == TestDataSeeder.TechnologyCategoryId);

        // Act - Get published posts in Technology category
        var postsResponse = await _unauthenticatedClient.GetAsync($"/api/posts/category/{TestDataSeeder.TechnologyCategoryId}");
        var publishedPosts = await postsResponse.Content.ReadFromJsonAsync<PostResponse[]>();

        // Assert - PostCount includes all posts, published posts endpoint only shows published
        // So PostCount should be >= published posts (may include drafts)
        techCategory.PostCount.Should().BeGreaterThanOrEqualTo(publishedPosts.Length);
        
        // Verify that we have some posts in this category
        techCategory.PostCount.Should().BeGreaterThan(0);
        publishedPosts.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPostsByNonExistentCategory_ShouldReturnEmptyList()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();

        // Act
        var response = await _unauthenticatedClient.GetAsync($"/api/posts/category/{nonExistentCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var posts = await response.Content.ReadFromJsonAsync<PostResponse[]>();
        posts.Should().NotBeNull();
        posts.Should().BeEmpty();
    }

    #endregion
}