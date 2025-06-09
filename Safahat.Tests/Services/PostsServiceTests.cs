using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Safahat.Application.DTOs.Requests.Posts;
using Safahat.Application.DTOs.Responses.Posts;
using Safahat.Application.Services;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Tests.Services;

public class PostServiceTests
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;
    private readonly PostService _postService;
    private readonly ISession _session;

    public PostServiceTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _tagRepository = Substitute.For<ITagRepository>();
        _mapper = Substitute.For<IMapper>();
        _session = Substitute.For<ISession>();
        
        _postService = new PostService(
            _postRepository,
            _categoryRepository,
            _tagRepository,
            _mapper
        );
    }

    [Fact]
    public async Task GetByIdAsync_WhenPostExists_ShouldReturnPostResponse()
    {
        // Arrange
        Guid postId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test Content",
            Slug = "test-post",
            Status = PostStatus.Published,
            AuthorId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedResponse = new PostResponse
        {
            Id = postId,
            Title = "Test Post",
            Content = "Test Content",
            Slug = "test-post"
        };

        _postRepository
            .GetByIdAsync(postId)
            .Returns(post);

        _mapper
            .Map<PostResponse>(post)
            .Returns(expectedResponse);

        // Act
        var result = await _postService.GetByIdAsync(postId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        result.Id.Should().Be(postId);
        result.Title.Should().Be("Test Post");
        result.Content.Should().Be("Test Content");
        result.Slug.Should().Be("test-post");

        await _postRepository.Received(1).GetByIdAsync(postId);
        _mapper.Received(1).Map<PostResponse>(post);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPostDoesNotExist_ShouldThrowApplicationException()
    {
        // Arrange
        Guid postId = Guid.NewGuid();
        
        _postRepository
            .GetByIdAsync(postId)
            .Returns((Post)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _postService.GetByIdAsync(postId)
        );

        exception.Message.Should().Be("Post not found");
        await _postRepository.Received(1).GetByIdAsync(postId);
        _mapper.DidNotReceive().Map<PostResponse>(Arg.Any<Post>());
    }
    
    [Fact]
    public async Task GetBySlugAsync_WhenPostExists_ShouldReturnPostResponseAndIncrementViewCount()
    {
        // Arrange
        var slug = "test-post";
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Test Post",
            Content = "Test Content",
            Slug = slug,
            ViewCount = 5
        };

        var expectedResponse = new PostResponse
        {
            Id = post.Id,
            Title = "Test Post",
            Content = "Test Content",
            Slug = slug,
            ViewCount = 6
        };

        _postRepository.GetPostBySlugAsync(slug).Returns(post);
        _mapper.Map<PostResponse>(Arg.Any<Post>()).Returns(expectedResponse);

        // Act
        var result = await _postService.GetBySlugAsync(slug, _session);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        post.ViewCount.Should().Be(6);
        await _postRepository.Received(1).GetPostBySlugAsync(slug);
        await _postRepository.Received(1).UpdateAsync(post);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenPostDoesNotExist_ShouldThrowApplicationException()
    {
        // Arrange
        var slug = "non-existent-post";
        _postRepository.GetPostBySlugAsync(slug).Returns((Post)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _postService.GetBySlugAsync(slug, _session)
        );

        exception.Message.Should().Be("Post not found");
        await _postRepository.Received(1).GetPostBySlugAsync(slug);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPosts()
    {
        // Arrange
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Post 1" },
            new() { Id = Guid.NewGuid(), Title = "Post 2" }
        };

        var expectedResponses = new List<PostResponse>
        {
            new() { Id = posts[0].Id, Title = "Post 1" },
            new() { Id = posts[1].Id, Title = "Post 2" }
        };

        _postRepository.GetAllAsync().Returns(posts);
        _mapper.Map<IEnumerable<PostResponse>>(posts).Returns(expectedResponses);

        // Act
        var result = await _postService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponses);
        await _postRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePostAndReturnPostResponse()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var createRequest = new CreatePostRequest
        {
            Title = "New Post",
            Content = "Some content",
            IsDraft = false,
            CategoryIds = new List<Guid> { Guid.NewGuid() },
            Tags = new List<string> { "tag1", "tag2" }
        };

        var post = new Post
        {
            Title = "New Post",
            Content = "Some content",
            Status = PostStatus.Published
        };

        var category = new Category { Id = createRequest.CategoryIds[0] };
        var tag = new Tag { Id = Guid.NewGuid(), Name = "tag1", Slug = "tag1" };
        var createdPost = new Post
        {
            Id = Guid.NewGuid(),
            Title = "New Post",
            Content = "Some content",
            Status = PostStatus.Published,
            AuthorId = authorId,
            Slug = "new-post",
            PublishedAt = DateTime.UtcNow,
            PostCategories = new List<PostCategory>(),
            PostTags = new List<PostTag>()
        };

        var completePost = new Post
        {
            Id = createdPost.Id,
            Title = "New Post",
            Content = "Some content",
            Status = PostStatus.Published
        };

        var expectedResponse = new PostResponse
        {
            Id = createdPost.Id,
            Title = "New Post",
            Content = "Some content",
            Status = PostStatus.Published
        };

        _mapper.Map<Post>(createRequest).Returns(post);
        _postRepository.AddAsync(Arg.Any<Post>()).Returns(createdPost);
        _categoryRepository.GetByIdAsync(createRequest.CategoryIds[0]).Returns(category);
        _tagRepository.GetBySlugAsync("tag1").Returns(tag);
        _tagRepository.GetBySlugAsync("tag2").Returns((Tag)null);
        _tagRepository.AddAsync(Arg.Any<Tag>()).Returns(new Tag { Id = Guid.NewGuid(), Name = "tag2", Slug = "tag2" });
        _postRepository.GetByIdAsync(createdPost.Id).Returns(completePost);
        _mapper.Map<PostResponse>(completePost).Returns(expectedResponse);

        // Act
        var result = await _postService.CreateAsync(authorId, createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _postRepository.Received(1).AddAsync(Arg.Any<Post>());
        await _postRepository.Received(1).UpdateAsync(Arg.Any<Post>());
        await _postRepository.Received(1).GetByIdAsync(createdPost.Id);
    }

    [Fact] 
    public async Task UpdateAsync_WhenPostExists_ShouldUpdateAndReturnPost()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var updateRequest = new UpdatePostRequest
        {
            Title = "Updated Title",
            Content = "Updated content",
            CategoryIds = new List<Guid> { Guid.NewGuid() },
            Tags = new List<string> { "newtag" }
        };

        var existingPost = new Post
        {
            Id = postId,
            Title = "Original Title",
            Content = "Original content",
            Slug = "original-title",
            PostCategories = new List<PostCategory>(),
            PostTags = new List<PostTag>()
        };

        var updatedPost = new Post
        {
            Id = postId,
            Title = "Updated Title",
            Content = "Updated content",
            Slug = "updated-title"
        };

        var expectedResponse = new PostResponse
        {
            Id = postId,
            Title = "Updated Title",
            Content = "Updated content",
            Slug = "updated-title"
        };

        _postRepository.GetByIdAsync(postId).Returns(existingPost, updatedPost);
        _categoryRepository.GetByIdAsync(updateRequest.CategoryIds[0]).Returns(new Category { Id = updateRequest.CategoryIds[0] });
        _tagRepository.GetBySlugAsync("newtag").Returns((Tag)null);
        _tagRepository.AddAsync(Arg.Any<Tag>()).Returns(new Tag { Id = Guid.NewGuid(), Name = "newtag", Slug = "newtag" });
        _mapper.Map<PostResponse>(updatedPost).Returns(expectedResponse);

        // Act
        var result = await _postService.UpdateAsync(postId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _postRepository.Received(2).GetByIdAsync(postId);
        await _postRepository.Received(1).UpdateAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task UpdateAsync_WhenPostDoesNotExist_ShouldThrowApplicationException()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var updateRequest = new UpdatePostRequest { Title = "Updated Title" };
        
        _postRepository.GetByIdAsync(postId).Returns((Post)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _postService.UpdateAsync(postId, updateRequest)
        );

        exception.Message.Should().Be("Post not found");
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task DeleteAsync_WhenPostExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post { Id = postId };
        
        _postRepository.GetByIdAsync(postId).Returns(post);
        _postRepository.DeleteAsync(postId).Returns(Task.FromResult(true));
        
        // Act
        var result = await _postService.DeleteAsync(postId);

        // Assert
        result.Should().BeTrue();
        await _postRepository.Received(1).GetByIdAsync(postId);
        await _postRepository.Received(1).DeleteAsync(postId);
    }

    [Fact]
    public async Task DeleteAsync_WhenPostDoesNotExist_ShouldThrowApplicationException()
    {
        // Arrange
        var postId = Guid.NewGuid();
        
        _postRepository.GetByIdAsync(postId).Returns((Post)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _postService.DeleteAsync(postId)
        );

        exception.Message.Should().Be("Post not found");
        await _postRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task GetPublishedPostsAsync_ShouldReturnPaginatedPublishedPosts()
    {
        // Arrange
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Post 1", Status = PostStatus.Published },
            new() { Id = Guid.NewGuid(), Title = "Post 2", Status = PostStatus.Published },
            new() { Id = Guid.NewGuid(), Title = "Post 3", Status = PostStatus.Published }
        };

        var expectedResponses = new List<PostResponse>
        {
            new() { Id = posts[0].Id, Title = "Post 1" },
            new() { Id = posts[1].Id, Title = "Post 2" }
        };

        _postRepository.GetPublishedPostsAsync().Returns(posts);
        _mapper.Map<IEnumerable<PostResponse>>(Arg.Any<IEnumerable<Post>>()).Returns(expectedResponses);

        // Act
        var result = await _postService.GetPublishedPostsAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponses);
        await _postRepository.Received(1).GetPublishedPostsAsync();
    }

    [Fact]
    public async Task PublishPostAsync_WhenPostExists_ShouldPublishPostAndReturnTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            Status = PostStatus.Draft,
            PublishedAt = null
        };
        
        _postRepository.GetByIdAsync(postId).Returns(post);

        // Act
        var result = await _postService.PublishPostAsync(postId);

        // Assert
        result.Should().BeTrue();
        post.Status.Should().Be(PostStatus.Published);
        post.PublishedAt.Should().NotBeNull();
        post.UpdatedAt.Should().NotBeNull();
        await _postRepository.Received(1).GetByIdAsync(postId);
        await _postRepository.Received(1).UpdateAsync(post);
    }

    [Fact]
    public async Task PublishPostAsync_WhenPostDoesNotExist_ShouldThrowApplicationException()
    {
        // Arrange
        var postId = Guid.NewGuid();
        
        _postRepository.GetByIdAsync(postId).Returns((Post)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _postService.PublishPostAsync(postId)
        );

        exception.Message.Should().Be("Post not found");
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>());
    }

    [Fact]
    public async Task UnpublishPostAsync_WhenPostExists_ShouldUnpublishPostAndReturnTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            Status = PostStatus.Published
        };
        
        _postRepository.GetByIdAsync(postId).Returns(post);

        // Act
        var result = await _postService.UnpublishPostAsync(postId);

        // Assert
        result.Should().BeTrue();
        post.Status.Should().Be(PostStatus.Draft);
        post.UpdatedAt.Should().NotBeNull();
        await _postRepository.Received(1).GetByIdAsync(postId);
        await _postRepository.Received(1).UpdateAsync(post);
    }

    [Fact]
    public async Task FeaturePostAsync_WhenPostExists_ShouldFeaturePostAndReturnTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            IsFeatured = false
        };
        
        _postRepository.GetByIdAsync(postId).Returns(post);

        // Act
        var result = await _postService.FeaturePostAsync(postId);

        // Assert
        result.Should().BeTrue();
        post.IsFeatured.Should().BeTrue();
        post.UpdatedAt.Should().NotBeNull();
        await _postRepository.Received(1).GetByIdAsync(postId);
        await _postRepository.Received(1).UpdateAsync(post);
    }

    [Fact]
    public async Task UnfeaturePostAsync_WhenPostExists_ShouldUnfeaturePostAndReturnTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            IsFeatured = true
        };
        
        _postRepository.GetByIdAsync(postId).Returns(post);

        // Act
        var result = await _postService.UnfeaturePostAsync(postId);

        // Assert
        result.Should().BeTrue();
        post.IsFeatured.Should().BeFalse();
        post.UpdatedAt.Should().NotBeNull();
        await _postRepository.Received(1).GetByIdAsync(postId);
        await _postRepository.Received(1).UpdateAsync(post);
    }

    [Fact]
    public async Task GetPostsByAuthorAsync_ShouldReturnPaginatedPostsByAuthor()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Author Post 1", AuthorId = authorId },
            new() { Id = Guid.NewGuid(), Title = "Author Post 2", AuthorId = authorId },
            new() { Id = Guid.NewGuid(), Title = "Author Post 3", AuthorId = authorId }
        };

        var expectedResponses = new List<PostResponse>
        {
            new() { Id = posts[0].Id, Title = "Author Post 1" },
            new() { Id = posts[1].Id, Title = "Author Post 2" }
        };

        _postRepository.GetPostsByAuthorAsync(authorId).Returns(posts);
        _mapper.Map<IEnumerable<PostResponse>>(Arg.Any<IEnumerable<Post>>()).Returns(expectedResponses);

        // Act
        var result = await _postService.GetPostsByAuthorAsync(authorId, 1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponses);
        await _postRepository.Received(1).GetPostsByAuthorAsync(authorId);
    }

    [Fact]
    public async Task GetFeaturedPostsAsync_ShouldReturnFeaturedPosts()
    {
        // Arrange
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Featured Post 1", IsFeatured = true },
            new() { Id = Guid.NewGuid(), Title = "Featured Post 2", IsFeatured = true }
        };

        var expectedResponses = new List<PostResponse>
        {
            new() { Id = posts[0].Id, Title = "Featured Post 1" },
            new() { Id = posts[1].Id, Title = "Featured Post 2" }
        };

        _postRepository.GetFeaturedPostsAsync().Returns(posts);
        _mapper.Map<IEnumerable<PostResponse>>(posts).Returns(expectedResponses);

        // Act
        var result = await _postService.GetFeaturedPostsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponses);
        await _postRepository.Received(1).GetFeaturedPostsAsync();
    }

    [Fact]
    public async Task SearchPostsAsync_ShouldReturnPaginatedSearchResults()
    {
        // Arrange
        var searchTerm = "test";
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Test Post 1" },
            new() { Id = Guid.NewGuid(), Title = "Test Post 2" },
            new() { Id = Guid.NewGuid(), Title = "Test Post 3" }
        };

        var expectedResponses = new List<PostResponse>
        {
            new() { Id = posts[0].Id, Title = "Test Post 1" },
            new() { Id = posts[1].Id, Title = "Test Post 2" }
        };

        _postRepository.SearchPostsAsync(searchTerm).Returns(posts);
        _mapper.Map<IEnumerable<PostResponse>>(Arg.Any<IEnumerable<Post>>()).Returns(expectedResponses);

        // Act
        var result = await _postService.SearchPostsAsync(searchTerm, 1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponses);
        await _postRepository.Received(1).SearchPostsAsync(searchTerm);
    }

    [Fact]
    public async Task GetPostsByCategoryAsync_ShouldReturnPaginatedPostsByCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Category Post 1" },
            new() { Id = Guid.NewGuid(), Title = "Category Post 2" },
            new() { Id = Guid.NewGuid(), Title = "Category Post 3" }
        };

        var expectedResponses = new List<PostResponse>
        {
            new() { Id = posts[0].Id, Title = "Category Post 1" },
            new() { Id = posts[1].Id, Title = "Category Post 2" }
        };

        _postRepository.GetPostsByCategoryAsync(categoryId).Returns(posts);
        _mapper.Map<IEnumerable<PostResponse>>(Arg.Any<IEnumerable<Post>>()).Returns(expectedResponses);

        // Act
        var result = await _postService.GetPostsByCategoryAsync(categoryId, 1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponses);
        await _postRepository.Received(1).GetPostsByCategoryAsync(categoryId);
    }

    [Fact]
    public async Task GetPostsByTagAsync_ShouldReturnPaginatedPostsByTag()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Tag Post 1" },
            new() { Id = Guid.NewGuid(), Title = "Tag Post 2" },
            new() { Id = Guid.NewGuid(), Title = "Tag Post 3" }
        };

        var expectedResponses = new List<PostResponse>
        {
            new() { Id = posts[0].Id, Title = "Tag Post 1" },
            new() { Id = posts[1].Id, Title = "Tag Post 2" }
        };

        _postRepository.GetPostsByTagAsync(tagId).Returns(posts);
        _mapper.Map<IEnumerable<PostResponse>>(Arg.Any<IEnumerable<Post>>()).Returns(expectedResponses);

        // Act
        var result = await _postService.GetPostsByTagAsync(tagId, 1, 2);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponses);
        await _postRepository.Received(1).GetPostsByTagAsync(tagId);
    }
}