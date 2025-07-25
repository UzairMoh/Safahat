using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Comments;
using Safahat.Application.DTOs.Responses.Posts;
using Safahat.Tests.Integration.Infrastructure;

namespace Safahat.Tests.Integration.Scenarios;

/// <summary>
/// Integration tests for complete post creation and management workflows
/// Tests multi-step processes that span across multiple controllers and business operations
/// </summary>
public class PostCreationWorkflowTests : IClassFixture<SafahatWebApplicationFactory>
{
    private readonly SafahatWebApplicationFactory _factory;

    public PostCreationWorkflowTests(SafahatWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompletePostLifecycle_CreateDraftToPublishedWithComments_ShouldWorkEndToEnd()
    {
        // Arrange
        var authorClient = _factory.CreateAuthorClient();
        var readerClient = _factory.CreateReaderClient();
        var adminClient = _factory.CreateAdminClient();
        var publicClient = _factory.CreateUnauthenticatedClient();

        // Step 1: Author creates a draft post
        var createRequest = new CreatePostRequest
        {
            Title = "Complete Workflow Test Post",
            Content = "This post will go through the complete lifecycle from draft to published with comments.",
            Summary = "A comprehensive workflow test",
            IsDraft = true, // Start as draft
            CategoryIds = new List<Guid> { TestDataSeeder.TechnologyCategoryId },
            Tags = new List<string> { "workflow", "testing", "integration" }
        };

        var createResponse = await authorClient.PostAsJsonAsync("/api/posts", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPost = await createResponse.Content.ReadFromJsonAsync<PostResponse>();
        createdPost.Should().NotBeNull();
        createdPost.Status.Should().Be(Models.Enums.PostStatus.Draft);

        // Step 2: Verify draft is NOT visible in public feed
        var publicFeedResponse = await publicClient.GetAsync("/api/posts/published");
        var publicPosts = await publicFeedResponse.Content.ReadFromJsonAsync<PostResponse[]>();
        publicPosts.Should().NotContain(p => p.Id == createdPost.Id);

        // Step 3: Author updates the post content
        var updateRequest = new UpdatePostRequest
        {
            Title = "Updated Complete Workflow Test Post",
            Content = "This content has been updated and improved for the workflow test.",
            Summary = "An updated comprehensive workflow test",
            CategoryIds = new List<Guid> { TestDataSeeder.TechnologyCategoryId, TestDataSeeder.LifestyleCategoryId },
            Tags = new List<string> { "workflow", "testing", "integration", "updated" }
        };

        var updateResponse = await authorClient.PutAsJsonAsync($"/api/posts/{createdPost.Id}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPost = await updateResponse.Content.ReadFromJsonAsync<PostResponse>();
        updatedPost.Title.Should().Be(updateRequest.Title);
        updatedPost.Categories.Should().HaveCount(2);
        updatedPost.Tags.Should().HaveCount(4);

        // Step 4: Author publishes the post
        var publishResponse = await authorClient.PutAsync($"/api/posts/{createdPost.Id}/publish", null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 5: Verify post is now visible in public feed
        var updatedPublicFeedResponse = await publicClient.GetAsync("/api/posts/published");
        var updatedPublicPosts = await updatedPublicFeedResponse.Content.ReadFromJsonAsync<PostResponse[]>();
        updatedPublicPosts.Should().Contain(p => p.Id == createdPost.Id);

        // Step 6: Reader adds a comment to the published post
        var commentRequest = new CreateCommentRequest
        {
            Content = "Great post! This workflow testing is very thorough.",
            PostId = createdPost.Id
        };

        var commentResponse = await readerClient.PostAsJsonAsync($"/api/posts/{createdPost.Id}/comments", commentRequest);
        commentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var comment = await commentResponse.Content.ReadFromJsonAsync<CommentResponse>();
        comment.Should().NotBeNull();
        comment.PostId.Should().Be(createdPost.Id);

        // Step 7: Admin features the post
        var featureResponse = await adminClient.PutAsync($"/api/posts/{createdPost.Id}/feature", null);
        featureResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 8: Verify post appears in featured posts
        var featuredPostsResponse = await publicClient.GetAsync("/api/posts/featured");
        var featuredPosts = await featuredPostsResponse.Content.ReadFromJsonAsync<PostResponse[]>();
        featuredPosts.Should().Contain(p => p.Id == createdPost.Id && p.IsFeatured == true);

        // Step 9: Verify final post state includes all changes
        var finalPostResponse = await publicClient.GetAsync($"/api/posts/{createdPost.Id}");
        var finalPost = await finalPostResponse.Content.ReadFromJsonAsync<PostResponse>();
        finalPost.Should().NotBeNull();
        finalPost.Title.Should().Be(updateRequest.Title);
        finalPost.Status.Should().Be(Models.Enums.PostStatus.Published);
        finalPost.IsFeatured.Should().BeTrue();
        finalPost.CommentCount.Should().BeGreaterThan(0);
        finalPost.Categories.Should().HaveCount(2);
        finalPost.Tags.Should().HaveCount(4);
    }

    [Fact]
    public async Task PostModerationWorkflow_AuthorToAdminToPublic_ShouldMaintainCorrectPermissions()
    {
        // Arrange
        var authorClient = _factory.CreateAuthorClient();
        var otherReaderClient = _factory.CreateOtherReaderClient();
        var adminClient = _factory.CreateAdminClient();

        // Step 1: Author creates a post
        var createRequest = new CreatePostRequest
        {
            Title = "Moderation Workflow Test",
            Content = "This post will test the moderation workflow and permissions.",
            Summary = "Testing moderation permissions",
            IsDraft = false,
            CategoryIds = new List<Guid> { TestDataSeeder.TechnologyCategoryId }
        };

        var createResponse = await authorClient.PostAsJsonAsync("/api/posts", createRequest);
        var createdPost = await createResponse.Content.ReadFromJsonAsync<PostResponse>();

        // Step 2: Author can edit their own post
        var authorUpdateRequest = new UpdatePostRequest
        {
            Title = "Updated by Author",
            Content = "Author successfully updated their own post."
        };

        var authorUpdateResponse = await authorClient.PutAsJsonAsync($"/api/posts/{createdPost.Id}", authorUpdateRequest);
        authorUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Other user CANNOT edit the post
        var otherUserUpdateRequest = new UpdatePostRequest
        {
            Title = "Unauthorized Update Attempt",
            Content = "This should not work."
        };

        var otherUserUpdateResponse = await otherReaderClient.PutAsJsonAsync($"/api/posts/{createdPost.Id}", otherUserUpdateRequest);
        otherUserUpdateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Step 4: Admin CAN edit any post
        var adminUpdateRequest = new UpdatePostRequest
        {
            Title = "Updated by Admin",
            Content = "Admin successfully updated the post for moderation."
        };

        var adminUpdateResponse = await adminClient.PutAsJsonAsync($"/api/posts/{createdPost.Id}", adminUpdateRequest);
        adminUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Admin can feature the post
        var featureResponse = await adminClient.PutAsync($"/api/posts/{createdPost.Id}/feature", null);
        featureResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 6: Non-admin cannot feature posts
        var userFeatureResponse = await authorClient.PutAsync($"/api/posts/{createdPost.Id}/unfeature", null);
        userFeatureResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Step 7: Verify final state reflects admin changes
        var finalPostResponse = await adminClient.GetAsync($"/api/posts/{createdPost.Id}");
        var finalPost = await finalPostResponse.Content.ReadFromJsonAsync<PostResponse>();
        finalPost.Title.Should().Be("Updated by Admin");
        finalPost.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public async Task PostWithCommentsAndReplies_CompleteInteractionWorkflow_ShouldMaintainHierarchy()
    {
        // Arrange
        var authorClient = _factory.CreateAuthorClient();
        var reader1Client = _factory.CreateReaderClient();
        var reader2Client = _factory.CreateOtherReaderClient();
        var adminClient = _factory.CreateAdminClient();

        // Step 1: Create and publish a post
        var createRequest = new CreatePostRequest
        {
            Title = "Discussion Post for Comments",
            Content = "This post is designed to generate discussion and test comment hierarchies.",
            Summary = "A post for testing comment interactions",
            IsDraft = false,
            CategoryIds = new List<Guid> { TestDataSeeder.LifestyleCategoryId }
        };

        var createResponse = await authorClient.PostAsJsonAsync("/api/posts", createRequest);
        var post = await createResponse.Content.ReadFromJsonAsync<PostResponse>();

        // Step 2: Reader 1 adds initial comment
        var comment1Request = new CreateCommentRequest
        {
            Content = "This is a great discussion starter! I have some thoughts on this topic.",
            PostId = post.Id
        };

        var comment1Response = await reader1Client.PostAsJsonAsync($"/api/posts/{post.Id}/comments", comment1Request);
        var comment1 = await comment1Response.Content.ReadFromJsonAsync<CommentResponse>();

        // Step 3: Reader 2 adds another top-level comment
        var comment2Request = new CreateCommentRequest
        {
            Content = "I agree with the points made in this post. Very insightful!",
            PostId = post.Id
        };

        var comment2Response = await reader2Client.PostAsJsonAsync($"/api/posts/{post.Id}/comments", comment2Request);
        comment2Response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 4: Author replies to Reader 1's comment
        var replyRequest = new CreateCommentRequest
        {
            Content = "Thank you for the thoughtful comment! I'm glad it resonated with you.",
            PostId = post.Id,
            ParentCommentId = comment1.Id
        };

        var replyResponse = await authorClient.PostAsJsonAsync($"/api/posts/{post.Id}/comments", replyRequest);
        replyResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 5: Admin moderates - approves all comments
        var commentsResponse = await adminClient.GetAsync($"/api/posts/{post.Id}/comments");
        var comments = await commentsResponse.Content.ReadFromJsonAsync<CommentResponse[]>();
        
        foreach (var comment in comments)
        {
            var approveResponse = await adminClient.PutAsync($"/api/comments/{comment.Id}/approve", null);
            approveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        // Step 6: Verify post shows updated comment count
        var updatedPostResponse = await reader1Client.GetAsync($"/api/posts/{post.Id}");
        var updatedPost = await updatedPostResponse.Content.ReadFromJsonAsync<PostResponse>();
        updatedPost.CommentCount.Should().BeGreaterThanOrEqualTo(3);

        // Step 7: Verify comment hierarchy is maintained
        var finalCommentsResponse = await reader1Client.GetAsync($"/api/posts/{post.Id}/comments");
        var finalComments = await finalCommentsResponse.Content.ReadFromJsonAsync<CommentResponse[]>();
        
        finalComments.Should().NotBeEmpty();
        finalComments.Should().Contain(c => c.ParentCommentId == null); // Top-level comments
        finalComments.Should().Contain(c => c.ParentCommentId == comment1.Id); // Reply to comment1
    }

    [Fact]
    public async Task PostCategoriesAndTagsWorkflow_CreateUpdateAndFilter_ShouldMaintainRelationships()
    {
        // Arrange
        var authorClient = _factory.CreateAuthorClient();
        var publicClient = _factory.CreateUnauthenticatedClient();

        // Step 1: Create post with specific categories and tags
        var createRequest = new CreatePostRequest
        {
            Title = "Multi-Category Multi-Tag Post",
            Content = "This post tests category and tag relationships throughout the workflow.",
            Summary = "Testing categories and tags",
            IsDraft = false,
            CategoryIds = new List<Guid> { TestDataSeeder.TechnologyCategoryId },
            Tags = new List<string> { "original", "first-version" }
        };

        var createResponse = await authorClient.PostAsJsonAsync("/api/posts", createRequest);
        var post = await createResponse.Content.ReadFromJsonAsync<PostResponse>();
        
        post.Categories.Should().HaveCount(1);
        post.Tags.Should().HaveCount(2);

        // Step 2: Update post with additional categories and tags
        var updateRequest = new UpdatePostRequest
        {
            Title = post.Title,
            Content = post.Content,
            CategoryIds = new List<Guid> { TestDataSeeder.TechnologyCategoryId, TestDataSeeder.LifestyleCategoryId },
            Tags = new List<string> { "updated", "multi-category", "workflow-test" }
        };

        var updateResponse = await authorClient.PutAsJsonAsync($"/api/posts/{post.Id}", updateRequest);
        var updatedPost = await updateResponse.Content.ReadFromJsonAsync<PostResponse>();
        
        updatedPost.Categories.Should().HaveCount(2);
        updatedPost.Tags.Should().HaveCount(3);

        // Step 3: Verify post appears in both category filters
        var tech_categoryResponse = await publicClient.GetAsync($"/api/posts/category/{TestDataSeeder.TechnologyCategoryId}");
        var techPosts = await tech_categoryResponse.Content.ReadFromJsonAsync<PostResponse[]>();
        techPosts.Should().Contain(p => p.Id == post.Id);

        var lifestyleCategoryResponse = await publicClient.GetAsync($"/api/posts/category/{TestDataSeeder.LifestyleCategoryId}");
        var lifestylePosts = await lifestyleCategoryResponse.Content.ReadFromJsonAsync<PostResponse[]>();
        lifestylePosts.Should().Contain(p => p.Id == post.Id);

        // Step 4: Verify searching by tag content works
        var tagSearchResponse = await publicClient.GetAsync("/api/posts/search?query=workflow-test");
        var tagSearchPosts = await tagSearchResponse.Content.ReadFromJsonAsync<PostResponse[]>();
        tagSearchPosts.Should().Contain(p => p.Id == post.Id);

        // Step 5: Update to remove some relationships
        var finalUpdateRequest = new UpdatePostRequest
        {
            Title = updatedPost.Title,
            Content = updatedPost.Content,
            CategoryIds = new List<Guid> { TestDataSeeder.TechnologyCategoryId }, // Remove lifestyle
            Tags = new List<string> { "final", "simplified" } // Replace all tags
        };

        var finalUpdateResponse = await authorClient.PutAsJsonAsync($"/api/posts/{post.Id}", finalUpdateRequest);
        var finalPost = await finalUpdateResponse.Content.ReadFromJsonAsync<PostResponse>();
        
        finalPost.Categories.Should().HaveCount(1);
        finalPost.Tags.Should().HaveCount(2);
        finalPost.Categories.First().Id.Should().Be(TestDataSeeder.TechnologyCategoryId);
    }
}