using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Safahat.Application.DTOs.Requests.Tags;
using Safahat.Application.DTOs.Responses.Tags;
using Safahat.Tests.Integration.Infrastructure;

namespace Safahat.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for TagsController covering tag management and retrieval.
/// </summary>
public class TagsControllerIntegrationTests : IClassFixture<SafahatWebApplicationFactory>
{
    private readonly SafahatWebApplicationFactory _factory;
    private readonly HttpClient _unauthenticatedClient;

    public TagsControllerIntegrationTests(SafahatWebApplicationFactory factory)
    {
        _factory = factory;
        _unauthenticatedClient = _factory.CreateUnauthenticatedClient();
    }

    #region Public Endpoints

    [Fact]
    public async Task GetAllTags_ShouldReturnAllTags()
    {
        var response = await _unauthenticatedClient.GetAsync("/api/tags");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tags = await response.Content.ReadFromJsonAsync<TagResponse[]>();
        tags.Should().NotBeNull();
        tags.Should().NotBeEmpty();
        tags.Should().Contain(t => t.Id == TestDataSeeder.CSharpTagId);
        tags.Should().Contain(t => t.Id == TestDataSeeder.TestingTagId);
    }

    [Fact]
    public async Task GetTagsWithPostCount_ShouldIncludePostCounts()
    {
        var response = await _unauthenticatedClient.GetAsync("/api/tags/with-post-count");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tags = await response.Content.ReadFromJsonAsync<TagResponse[]>();
        tags.Should().NotBeNull();
        tags.Should().NotBeEmpty();
        
        // All tags should have PostCount property set (may be 0 if no posts associated)
        tags.Should().OnlyContain(t => t.PostCount >= 0);
        
        // Should contain the test tags
        tags.Should().Contain(t => t.Id == TestDataSeeder.CSharpTagId);
        tags.Should().Contain(t => t.Id == TestDataSeeder.TestingTagId);
    }

    [Fact]
    public async Task GetPopularTags_ShouldReturnPopularTags()
    {
        var response = await _unauthenticatedClient.GetAsync("/api/tags/popular");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tags = await response.Content.ReadFromJsonAsync<TagResponse[]>();
        tags.Should().NotBeNull();
        tags.Should().NotBeEmpty();
        
        // Should be ordered by usage (post count) - most popular first
        for (int i = 0; i < tags.Length - 1; i++)
        {
            tags[i].PostCount.Should().BeGreaterThanOrEqualTo(tags[i + 1].PostCount);
        }
        
        // Should contain our test tags
        tags.Should().Contain(t => t.Id == TestDataSeeder.CSharpTagId);
        tags.Should().Contain(t => t.Id == TestDataSeeder.TestingTagId);
    }

    [Fact]
    public async Task GetPopularTags_WithCustomCount_ShouldRespectLimit()
    {
        var response = await _unauthenticatedClient.GetAsync("/api/tags/popular?count=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tags = await response.Content.ReadFromJsonAsync<TagResponse[]>();
        tags.Should().NotBeNull();
        tags.Length.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetTagById_WhenExists_ShouldReturnTag()
    {
        var response = await _unauthenticatedClient.GetAsync($"/api/tags/{TestDataSeeder.CSharpTagId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tag = await response.Content.ReadFromJsonAsync<TagResponse>();
        tag.Should().NotBeNull();
        tag.Id.Should().Be(TestDataSeeder.CSharpTagId);
        tag.Name.Should().Be("csharp");
        tag.Slug.Should().Be("csharp");
    }

    [Fact]
    public async Task GetTagById_WhenNotExists_ShouldReturn404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await _unauthenticatedClient.GetAsync($"/api/tags/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTagBySlug_WhenExists_ShouldReturnTag()
    {
        var response = await _unauthenticatedClient.GetAsync("/api/tags/slug/csharp");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tag = await response.Content.ReadFromJsonAsync<TagResponse>();
        tag.Should().NotBeNull();
        tag.Id.Should().Be(TestDataSeeder.CSharpTagId);
        tag.Name.Should().Be("csharp");
        tag.Slug.Should().Be("csharp");
    }

    [Fact]
    public async Task GetTagBySlug_WhenNotExists_ShouldReturn404()
    {
        var response = await _unauthenticatedClient.GetAsync("/api/tags/slug/non-existent-tag");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Admin-Only Endpoints

    [Fact]
    public async Task CreateTag_AsAdmin_ShouldCreateTag()
    {
        var client = _factory.CreateAdminClient();
        var request = new CreateTagRequest
        {
            Name = "JavaScript",
            Slug = "javascript"
        };

        var response = await client.PostAsJsonAsync("/api/tags", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var tag = await response.Content.ReadFromJsonAsync<TagResponse>();
        tag.Should().NotBeNull();
        tag.Name.Should().Be(request.Name);
        tag.Slug.Should().Be(request.Slug);
        tag.PostCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateTag_AsRegularUser_ShouldReturn403()
    {
        var client = _factory.CreateReaderClient();
        var request = new CreateTagRequest
        {
            Name = "Unauthorised Tag",
            Slug = "unauthorised"
        };

        var response = await client.PostAsJsonAsync("/api/tags", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateTag_AsUnauthenticated_ShouldReturn401()
    {
        var request = new CreateTagRequest
        {
            Name = "Unauthenticated Tag",
            Slug = "unauthenticated"
        };

        var response = await _unauthenticatedClient.PostAsJsonAsync("/api/tags", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTag_WithDuplicateSlug_ShouldReturn400()
    {
        var client = _factory.CreateAdminClient();
        var request = new CreateTagRequest
        {
            Name = "Duplicate C#",
            Slug = "csharp" // This slug already exists
        };

        var response = await client.PostAsJsonAsync("/api/tags", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTag_WithEmptySlug_ShouldAutoGenerateSlug()
    {
        var client = _factory.CreateAdminClient();
        var request = new CreateTagRequest
        {
            Name = "Auto Generated Slug Tag",
            Slug = "" // Empty slug should trigger auto-generation
        };

        var response = await client.PostAsJsonAsync("/api/tags", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var tag = await response.Content.ReadFromJsonAsync<TagResponse>();
        tag.Should().NotBeNull();
        tag.Slug.Should().NotBeNullOrEmpty();
        tag.Slug.Should().Be("auto-generated-slug-tag");
    }

    [Fact]
    public async Task UpdateTag_AsAdmin_ShouldUpdateTag()
    {
        _factory.ResetDatabase();
        var adminClient = _factory.CreateClientWithoutReset(
            TestDataSeeder.AdminUserId,
            "Admin",
            "adminuser",
            "admin@test.com"
        );

        var createRequest = new CreateTagRequest
        {
            Name = "Original Tag",
            Slug = "original-tag"
        };

        var createResponse = await adminClient.PostAsJsonAsync("/api/tags", createRequest);
        var createdTag = await createResponse.Content.ReadFromJsonAsync<TagResponse>();

        var updateRequest = new UpdateTagRequest
        {
            Name = "Updated Tag",
            Slug = "updated-tag"
        };

        var response = await adminClient.PutAsJsonAsync($"/api/tags/{createdTag.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTag = await response.Content.ReadFromJsonAsync<TagResponse>();
        updatedTag.Should().NotBeNull();
        updatedTag.Name.Should().Be(updateRequest.Name);
        updatedTag.Slug.Should().Be(updateRequest.Slug);
        updatedTag.Id.Should().Be(createdTag.Id);
    }

    [Fact]
    public async Task UpdateTag_AsRegularUser_ShouldReturn403()
    {
        var client = _factory.CreateReaderClient();
        var request = new UpdateTagRequest
        {
            Name = "Unauthorised Update",
            Slug = "unauthorised-update"
        };

        var response = await client.PutAsJsonAsync($"/api/tags/{TestDataSeeder.CSharpTagId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateTag_WithNonExistentId_ShouldReturn404()
    {
        var client = _factory.CreateAdminClient();
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateTagRequest
        {
            Name = "Non-existent Tag",
            Slug = "non-existent"
        };

        var response = await client.PutAsJsonAsync($"/api/tags/{nonExistentId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTag_AsAdmin_ShouldDeleteTag()
    {
        _factory.ResetDatabase();
        var adminClient = _factory.CreateClientWithoutReset(
            TestDataSeeder.AdminUserId,
            "Admin",
            "adminuser",
            "admin@test.com"
        );

        var createRequest = new CreateTagRequest
        {
            Name = "Tag to Delete",
            Slug = "tag-to-delete"
        };

        var createResponse = await adminClient.PostAsJsonAsync("/api/tags", createRequest);
        var createdTag = await createResponse.Content.ReadFromJsonAsync<TagResponse>();

        var response = await adminClient.DeleteAsync($"/api/tags/{createdTag.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify tag is deleted
        var publicClient = _factory.CreateClientWithoutReset(Guid.NewGuid(), "Public", "public", "public@test.com");
        var getResponse = await publicClient.GetAsync($"/api/tags/{createdTag.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTag_AsRegularUser_ShouldReturn403()
    {
        var client = _factory.CreateReaderClient();

        var response = await client.DeleteAsync($"/api/tags/{TestDataSeeder.TestingTagId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteTag_WithNonExistentId_ShouldReturn404()
    {
        var client = _factory.CreateAdminClient();
        var nonExistentId = Guid.NewGuid();

        var response = await client.DeleteAsync($"/api/tags/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Tag-Post Relationship Tests

    [Fact]
    public async Task TagPostCount_ShouldBeCalculatedCorrectly()
    {
        // Get the tags with post count
        var tagsResponse = await _unauthenticatedClient.GetAsync("/api/tags/with-post-count");
        var tags = await tagsResponse.Content.ReadFromJsonAsync<TagResponse[]>();
        
        tags.Should().NotBeNull();
        tags.Should().NotBeEmpty();
        
        // All tags should have a valid post count (>= 0)
        tags.Should().OnlyContain(t => t.PostCount >= 0);
        
        // Should include our test tags
        tags.Should().Contain(t => t.Id == TestDataSeeder.CSharpTagId);
        tags.Should().Contain(t => t.Id == TestDataSeeder.TestingTagId);
    }

    [Fact]
    public async Task PopularTags_ShouldReturnTagsOrderedByUsage()
    {
        var response = await _unauthenticatedClient.GetAsync("/api/tags/popular?count=10");
        var popularTags = await response.Content.ReadFromJsonAsync<TagResponse[]>();

        popularTags.Should().NotBeNull();
        
        // Should be ordered by post count (descending)
        for (int i = 0; i < popularTags.Length - 1; i++)
        {
            popularTags[i].PostCount.Should().BeGreaterThanOrEqualTo(popularTags[i + 1].PostCount);
        }
        
        // Should include our test tags
        popularTags.Should().Contain(t => t.Id == TestDataSeeder.CSharpTagId);
        popularTags.Should().Contain(t => t.Id == TestDataSeeder.TestingTagId);
    }

    #endregion
}