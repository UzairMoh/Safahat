using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Safahat.Application.DTOs.Requests.Comments;
using Safahat.Application.DTOs.Responses.Comments;
using Safahat.Tests.Integration.Infrastructure;

namespace Safahat.Tests.Integration.Controllers;

/// <summary>
/// Integration tests for CommentsController covering comment creation, management and hierarchies.
/// </summary>
public class CommentsControllerIntegrationTests : IClassFixture<SafahatWebApplicationFactory>
{
    private readonly SafahatWebApplicationFactory _factory;
    private readonly HttpClient _unauthenticatedClient;

    public CommentsControllerIntegrationTests(SafahatWebApplicationFactory factory)
    {
        _factory = factory;
        _unauthenticatedClient = _factory.CreateUnauthenticatedClient();
    }

    #region Public Endpoints

    [Fact]
    public async Task GetCommentsByPost_ShouldReturnCommentsForPost()
    {
        var response = await _unauthenticatedClient.GetAsync($"/api/comments/post/{TestDataSeeder.PublishedPostId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<CommentResponse[]>();
        comments.Should().NotBeNull();
        comments.Should().OnlyContain(c => c.PostId == TestDataSeeder.PublishedPostId);
        comments.Should().Contain(c => c.Id == TestDataSeeder.ApprovedCommentId);
    }

    [Fact]
    public async Task GetCommentsByPost_WithNonExistentPost_ShouldReturnEmptyList()
    {
        var nonExistentPostId = Guid.NewGuid();

        var response = await _unauthenticatedClient.GetAsync($"/api/comments/post/{nonExistentPostId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<CommentResponse[]>();
        comments.Should().NotBeNull();
        comments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCommentById_WhenExists_ShouldReturnComment()
    {
        var response = await _unauthenticatedClient.GetAsync($"/api/comments/{TestDataSeeder.ApprovedCommentId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        comment.Should().NotBeNull();
        comment.Id.Should().Be(TestDataSeeder.ApprovedCommentId);
        comment.PostId.Should().Be(TestDataSeeder.PublishedPostId);
        comment.User.Should().NotBeNull();
        comment.User.Id.Should().Be(TestDataSeeder.OtherReaderUserId);
    }

    [Fact]
    public async Task GetCommentById_WhenNotExists_ShouldReturn404()
    {
        var nonExistentId = Guid.NewGuid();

        var response = await _unauthenticatedClient.GetAsync($"/api/comments/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Authenticated User Endpoints

    [Fact]
    public async Task CreateComment_AsAuthenticatedUser_ShouldCreateComment()
    {
        var client = _factory.CreateReaderClient();
        var request = new CreateCommentRequest
        {
            Content = "This is a test comment from integration tests",
            PostId = TestDataSeeder.PublishedPostId
        };

        var response = await client.PostAsJsonAsync("/api/comments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        comment.Should().NotBeNull();
        comment.Content.Should().Be(request.Content);
        comment.PostId.Should().Be(request.PostId);
        comment.User.Id.Should().Be(TestDataSeeder.ReaderUserId);
        comment.ParentCommentId.Should().BeNull();
    }

    [Fact]
    public async Task CreateComment_AsUnauthenticated_ShouldReturn401()
    {
        var request = new CreateCommentRequest
        {
            Content = "This should fail",
            PostId = TestDataSeeder.PublishedPostId
        };

        var response = await _unauthenticatedClient.PostAsJsonAsync("/api/comments", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateComment_OnNonExistentPost_ShouldReturn400()
    {
        var client = _factory.CreateReaderClient();
        var request = new CreateCommentRequest
        {
            Content = "Comment on non-existent post",
            PostId = Guid.NewGuid()
        };

        var response = await client.PostAsJsonAsync("/api/comments", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ReplyToComment_AsAuthenticatedUser_ShouldCreateReply()
    {
        var client = _factory.CreateAuthorClient();
        var request = new CreateCommentRequest
        {
            Content = "This is a reply to the existing comment"
        };

        var response = await client.PostAsJsonAsync($"/api/comments/{TestDataSeeder.ApprovedCommentId}/reply", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var reply = await response.Content.ReadFromJsonAsync<CommentResponse>();
        reply.Should().NotBeNull();
        reply.Content.Should().Be(request.Content);
        reply.PostId.Should().Be(TestDataSeeder.PublishedPostId);
        reply.ParentCommentId.Should().Be(TestDataSeeder.ApprovedCommentId);
        reply.User.Id.Should().Be(TestDataSeeder.AuthorUserId);
    }

    [Fact]
    public async Task ReplyToComment_ToNonExistentComment_ShouldReturn400()
    {
        var client = _factory.CreateReaderClient();
        var nonExistentCommentId = Guid.NewGuid();
        var request = new CreateCommentRequest
        {
            Content = "Reply to non-existent comment"
        };

        var response = await client.PostAsJsonAsync($"/api/comments/{nonExistentCommentId}/reply", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateComment_AsCommentAuthor_ShouldUpdateComment()
    {
        _factory.ResetDatabase();
        
        var client = _factory.CreateClientWithoutReset(
            TestDataSeeder.ReaderUserId,
            "Reader", 
            "readeruser",
            "reader@test.com"
        );
        
        var createRequest = new CreateCommentRequest
        {
            Content = "Original comment content",
            PostId = TestDataSeeder.PublishedPostId
        };

        var createResponse = await client.PostAsJsonAsync("/api/comments", createRequest);
        var createdComment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        var updateRequest = new UpdateCommentRequest
        {
            Content = "Updated comment content"
        };

        var response = await client.PutAsJsonAsync($"/api/comments/{createdComment.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedComment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        updatedComment.Should().NotBeNull();
        updatedComment.Content.Should().Be(updateRequest.Content);
        updatedComment.Id.Should().Be(createdComment.Id);
    }

    [Fact]
    public async Task UpdateComment_AsOtherUser_ShouldReturn400()
    {
        var client = _factory.CreateReaderClient();
        var updateRequest = new UpdateCommentRequest
        {
            Content = "Trying to update someone else's comment"
        };

        var response = await client.PutAsJsonAsync($"/api/comments/{TestDataSeeder.ApprovedCommentId}", updateRequest);

        // Authorisation errors in this system return 400, not 403
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteComment_AsCommentAuthor_ShouldDeleteComment()
    {
        _factory.ResetDatabase();
        
        var client = _factory.CreateClientWithoutReset(
            TestDataSeeder.ReaderUserId,
            "Reader",
            "readeruser",
            "reader@test.com"
        );
        
        var createRequest = new CreateCommentRequest
        {
            Content = "Comment to be deleted",
            PostId = TestDataSeeder.PublishedPostId
        };

        var createResponse = await client.PostAsJsonAsync("/api/comments", createRequest);
        var createdComment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        var response = await client.DeleteAsync($"/api/comments/{createdComment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify comment is deleted
        var publicClient = _factory.CreateClientWithoutReset(Guid.NewGuid(), "Public", "public", "public@test.com");
        var getResponse = await publicClient.GetAsync($"/api/comments/{createdComment.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteComment_AsOtherUser_ShouldReturn400()
    {
        var client = _factory.CreateReaderClient();

        var response = await client.DeleteAsync($"/api/comments/{TestDataSeeder.ApprovedCommentId}");

        // Authorisation errors in this system return 400, not 403
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteComment_AsAdmin_ShouldDeleteAnyComment()
    {
        _factory.ResetDatabase();
        
        var userClient = _factory.CreateClientWithoutReset(
            TestDataSeeder.OtherReaderUserId, 
            "Reader", 
            "otherreader", 
            "other@test.com"
        );
        
        var createRequest = new CreateCommentRequest
        {
            Content = "Comment for admin to delete",
            PostId = TestDataSeeder.PublishedPostId
        };

        var createResponse = await userClient.PostAsJsonAsync("/api/comments", createRequest);
        var createdComment = await createResponse.Content.ReadFromJsonAsync<CommentResponse>();

        var adminClient = _factory.CreateClientWithoutReset(
            TestDataSeeder.AdminUserId,
            "Admin",
            "adminuser", 
            "admin@test.com"
        );

        var response = await adminClient.DeleteAsync($"/api/comments/{createdComment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify comment is deleted
        var publicClient = _factory.CreateClientWithoutReset(Guid.NewGuid(), "Public", "public", "public@test.com");
        var getResponse = await publicClient.GetAsync($"/api/comments/{createdComment.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Admin-Only Endpoints

    [Fact]
    public async Task GetAllComments_AsAdmin_ShouldReturnAllComments()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.GetAsync("/api/comments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<CommentResponse[]>();
        comments.Should().NotBeNull();
        comments.Should().NotBeEmpty();
        comments.Should().Contain(c => c.Id == TestDataSeeder.ApprovedCommentId);
        comments.Should().Contain(c => c.Id == TestDataSeeder.PendingCommentId);
    }

    [Fact]
    public async Task GetAllComments_AsRegularUser_ShouldReturn403()
    {
        var client = _factory.CreateReaderClient();

        var response = await client.GetAsync("/api/comments");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCommentsByUser_AsOwner_ShouldReturnOwnComments()
    {
        var client = _factory.CreateOtherReaderClient();

        var response = await client.GetAsync($"/api/comments/user/{TestDataSeeder.OtherReaderUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<CommentResponse[]>();
        comments.Should().NotBeNull();
        comments.Should().OnlyContain(c => c.User.Id == TestDataSeeder.OtherReaderUserId);
        comments.Should().Contain(c => c.Id == TestDataSeeder.ApprovedCommentId);
    }

    [Fact]
    public async Task GetCommentsByUser_AsAdmin_ShouldReturnUserComments()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.GetAsync($"/api/comments/user/{TestDataSeeder.OtherReaderUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var comments = await response.Content.ReadFromJsonAsync<CommentResponse[]>();
        comments.Should().NotBeNull();
        comments.Should().OnlyContain(c => c.User.Id == TestDataSeeder.OtherReaderUserId);
    }

    [Fact]
    public async Task GetCommentsByUser_AsOtherUser_ShouldReturn403()
    {
        var client = _factory.CreateReaderClient();

        var response = await client.GetAsync($"/api/comments/user/{TestDataSeeder.OtherReaderUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Comment Hierarchy Tests

    [Fact]
    public async Task CommentReplies_ShouldMaintainHierarchy()
    {
        var client = _factory.CreateReaderClient();

        // Create a top-level comment
        var parentRequest = new CreateCommentRequest
        {
            Content = "Parent comment for hierarchy test",
            PostId = TestDataSeeder.PublishedPostId
        };

        var parentResponse = await client.PostAsJsonAsync("/api/comments", parentRequest);
        var parentComment = await parentResponse.Content.ReadFromJsonAsync<CommentResponse>();

        // Create a reply to the parent comment
        var replyRequest = new CreateCommentRequest
        {
            Content = "Reply to parent comment"
        };

        var replyResponse = await client.PostAsJsonAsync($"/api/comments/{parentComment.Id}/reply", replyRequest);
        var replyComment = await replyResponse.Content.ReadFromJsonAsync<CommentResponse>();

        // Create another reply to the same parent
        var secondReplyRequest = new CreateCommentRequest
        {
            Content = "Second reply to parent comment"
        };

        var secondReplyResponse = await client.PostAsJsonAsync($"/api/comments/{parentComment.Id}/reply", secondReplyRequest);
        var secondReplyComment = await secondReplyResponse.Content.ReadFromJsonAsync<CommentResponse>();

        // Get all comments for the post
        var commentsResponse = await _unauthenticatedClient.GetAsync($"/api/comments/post/{TestDataSeeder.PublishedPostId}");
        var allComments = await commentsResponse.Content.ReadFromJsonAsync<CommentResponse[]>();

        allComments.Should().NotBeNull();
        
        // Should contain the parent comment
        var foundParent = allComments.Should().Contain(c => c.Id == parentComment.Id).Subject;
        foundParent.ParentCommentId.Should().BeNull();
        
        // Should contain both replies
        var foundReply1 = allComments.Should().Contain(c => c.Id == replyComment.Id).Subject;
        foundReply1.ParentCommentId.Should().Be(parentComment.Id);
        
        var foundReply2 = allComments.Should().Contain(c => c.Id == secondReplyComment.Id).Subject;
        foundReply2.ParentCommentId.Should().Be(parentComment.Id);
    }

    [Fact]
    public async Task NestedReplies_ShouldMaintainDeepHierarchy()
    {
        var client = _factory.CreateAuthorClient();

        // Create parent comment
        var parentRequest = new CreateCommentRequest
        {
            Content = "Top-level comment",
            PostId = TestDataSeeder.PublishedPostId
        };

        var parentResponse = await client.PostAsJsonAsync("/api/comments", parentRequest);
        var parentComment = await parentResponse.Content.ReadFromJsonAsync<CommentResponse>();

        // Create first-level reply
        var level1ReplyRequest = new CreateCommentRequest
        {
            Content = "First level reply"
        };

        var level1Response = await client.PostAsJsonAsync($"/api/comments/{parentComment.Id}/reply", level1ReplyRequest);
        var level1Reply = await level1Response.Content.ReadFromJsonAsync<CommentResponse>();

        // Create second-level reply (reply to the reply)
        var level2ReplyRequest = new CreateCommentRequest
        {
            Content = "Second level reply"
        };

        var level2Response = await client.PostAsJsonAsync($"/api/comments/{level1Reply.Id}/reply", level2ReplyRequest);
        var level2Reply = await level2Response.Content.ReadFromJsonAsync<CommentResponse>();

        // Verify the hierarchy
        var commentsResponse = await _unauthenticatedClient.GetAsync($"/api/comments/post/{TestDataSeeder.PublishedPostId}");
        var allComments = await commentsResponse.Content.ReadFromJsonAsync<CommentResponse[]>();

        var foundParent = allComments.Should().Contain(c => c.Id == parentComment.Id).Subject;
        foundParent.ParentCommentId.Should().BeNull();

        var foundLevel1 = allComments.Should().Contain(c => c.Id == level1Reply.Id).Subject;
        foundLevel1.ParentCommentId.Should().Be(parentComment.Id);

        var foundLevel2 = allComments.Should().Contain(c => c.Id == level2Reply.Id).Subject;
        foundLevel2.ParentCommentId.Should().Be(level1Reply.Id);
    }

    #endregion
}