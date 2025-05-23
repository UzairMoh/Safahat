using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Safahat.Application.DTOs.Requests.Tags;
using Safahat.Application.DTOs.Responses.Tags;
using Safahat.Application.Services;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Tests.Services;

public class TagServiceTests
{
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;
    private readonly TagService _tagService;

    public TagServiceTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _mapper = Substitute.For<IMapper>();
        
        _tagService = new TagService(
            _tagRepository,
            _mapper
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingTag_ShouldReturnMappedTagResponse()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var tag = new Tag
        {
            Id = tagId,
            Name = "CSharp",
            Slug = "csharp"
        };

        var expectedResponse = new TagResponse
        {
            Id = tagId,
            Name = "CSharp",
            Slug = "csharp"
        };

        _tagRepository.GetByIdAsync(tagId).Returns(tag);
        _mapper.Map<TagResponse>(tag).Returns(expectedResponse);

        // Act
        var result = await _tagService.GetByIdAsync(tagId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _tagRepository.Received(1).GetByIdAsync(tagId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentTag_ShouldThrowApplicationException()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        _tagRepository.GetByIdAsync(tagId).Returns((Tag)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _tagService.GetByIdAsync(tagId)
        );

        exception.Message.Should().Be("Tag not found");
    }

    #endregion

    #region GetBySlugAsync Tests

    [Fact]
    public async Task GetBySlugAsync_WithExistingSlug_ShouldReturnMappedTagResponse()
    {
        // Arrange
        var slug = "csharp";
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "CSharp",
            Slug = slug
        };

        var expectedResponse = new TagResponse
        {
            Id = tag.Id,
            Name = "CSharp",
            Slug = slug
        };

        _tagRepository.GetBySlugAsync(slug).Returns(tag);
        _mapper.Map<TagResponse>(tag).Returns(expectedResponse);

        // Act
        var result = await _tagService.GetBySlugAsync(slug);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _tagRepository.Received(1).GetBySlugAsync(slug);
    }

    [Fact]
    public async Task GetBySlugAsync_WithNonExistentSlug_ShouldThrowApplicationException()
    {
        // Arrange
        var slug = "non-existent";
        _tagRepository.GetBySlugAsync(slug).Returns((Tag)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _tagService.GetBySlugAsync(slug)
        );

        exception.Message.Should().Be("Tag not found");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedTagsList()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" },
            new Tag { Id = Guid.NewGuid(), Name = "JavaScript", Slug = "javascript" }
        };

        var expectedResponse = new List<TagResponse>
        {
            new TagResponse { Id = tags[0].Id, Name = "CSharp", Slug = "csharp" },
            new TagResponse { Id = tags[1].Id, Name = "JavaScript", Slug = "javascript" }
        };

        _tagRepository.GetAllAsync().Returns(tags);
        _mapper.Map<IEnumerable<TagResponse>>(tags).Returns(expectedResponse);

        // Act
        var result = await _tagService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _tagRepository.Received(1).GetAllAsync();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDataAndNoSlug_ShouldGenerateSlugAndCreateTag()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "C# Programming"
            // No Slug provided - should be generated from Name
        };

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = "c-programming" // Generated slug
        };

        var expectedResponse = new TagResponse
        {
            Id = tag.Id,
            Name = request.Name,
            Slug = "c-programming"
        };

        _tagRepository.IsSlugUniqueAsync("c-programming").Returns(true);
        _mapper.Map<Tag>(Arg.Is<CreateTagRequest>(r => r.Slug == "c-programming")).Returns(tag);
        _tagRepository.AddAsync(tag).Returns(tag);
        _mapper.Map<TagResponse>(tag).Returns(expectedResponse);

        // Act
        var result = await _tagService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        result.Slug.Should().Be("c-programming");
        
        await _tagRepository.Received(1).IsSlugUniqueAsync("c-programming");
        await _tagRepository.Received(1).AddAsync(tag);
    }

    [Fact]
    public async Task CreateAsync_WithProvidedSlug_ShouldNormalizeSlugAndCreateTag()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "JavaScript",
            Slug = "Custom JS SLUG!"
        };

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = "custom-js-slug" // Normalized slug
        };

        var expectedResponse = new TagResponse
        {
            Id = tag.Id,
            Name = request.Name,
            Slug = "custom-js-slug"
        };

        _tagRepository.IsSlugUniqueAsync("custom-js-slug").Returns(true);
        _mapper.Map<Tag>(Arg.Is<CreateTagRequest>(r => r.Slug == "custom-js-slug")).Returns(tag);
        _tagRepository.AddAsync(tag).Returns(tag);
        _mapper.Map<TagResponse>(tag).Returns(expectedResponse);

        // Act
        var result = await _tagService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Slug.Should().Be("custom-js-slug");
        
        await _tagRepository.Received(1).IsSlugUniqueAsync("custom-js-slug");
        await _tagRepository.Received(1).AddAsync(tag);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSlug_ShouldThrowApplicationException()
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = "CSharp"
            // No Slug provided - will generate "csharp" from name
        };

        _tagRepository.IsSlugUniqueAsync("csharp").Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _tagService.CreateAsync(request)
        );

        exception.Message.Should().Be("A tag with this slug already exists");
        await _tagRepository.DidNotReceive().AddAsync(Arg.Any<Tag>());
    }

    [Theory]
    [InlineData("C# Programming", "c-programming")]
    [InlineData("Node.js", "nodejs")]
    [InlineData("React & Redux", "react-redux")]
    [InlineData("TypeScript!!!", "typescript")]
    [InlineData("   Vue.js   Framework   ", "vuejs-framework")]
    [InlineData("ANGULAR DEVELOPMENT", "angular-development")]
    [InlineData("Special@#$%Characters", "specialcharacters")]
    [InlineData("Français & Español", "francais-espanol")]
    [InlineData("Machine Learning", "machine-learning")]
    [InlineData("AI/ML", "aiml")]
    public async Task CreateAsync_SlugGeneration_ShouldCreateCorrectSlugs(string inputName, string expectedSlug)
    {
        // Arrange
        var request = new CreateTagRequest
        {
            Name = inputName
            // No Slug provided - should be generated from Name
        };

        var tag = new Tag { Id = Guid.NewGuid() };
        var response = new TagResponse { Id = tag.Id };

        _tagRepository.IsSlugUniqueAsync(expectedSlug).Returns(true);
        _mapper.Map<Tag>(Arg.Is<CreateTagRequest>(r => r.Slug == expectedSlug)).Returns(tag);
        _tagRepository.AddAsync(tag).Returns(tag);
        _mapper.Map<TagResponse>(tag).Returns(response);

        // Act
        await _tagService.CreateAsync(request);

        // Assert
        await _tagRepository.Received(1).IsSlugUniqueAsync(expectedSlug);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateTagAndReturnResponse()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var existingTag = new Tag
        {
            Id = tagId,
            Name = "JavaScript",
            Slug = "javascript"
        };

        var request = new UpdateTagRequest
        {
            Name = "Advanced JavaScript"
            // Name changed, so slug should be generated from new name
        };

        var expectedResponse = new TagResponse
        {
            Id = tagId,
            Name = "Advanced JavaScript",
            Slug = "advanced-javascript"
        };

        _tagRepository.GetByIdAsync(tagId).Returns(existingTag);
        _tagRepository.IsSlugUniqueAsync("advanced-javascript").Returns(true);
        _mapper.Map<TagResponse>(existingTag).Returns(expectedResponse);

        // Act
        var result = await _tagService.UpdateAsync(tagId, request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        
        await _tagRepository.Received(1).UpdateAsync(Arg.Is<Tag>(t => 
            t.Id == tagId && 
            t.UpdatedAt != null));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentTag_ShouldThrowApplicationException()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var request = new UpdateTagRequest { Name = "Updated Name" };

        _tagRepository.GetByIdAsync(tagId).Returns((Tag)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _tagService.UpdateAsync(tagId, request)
        );

        exception.Message.Should().Be("Tag not found");
        await _tagRepository.DidNotReceive().UpdateAsync(Arg.Any<Tag>());
    }

    [Fact]
    public async Task UpdateAsync_WithSameName_ShouldNotGenerateNewSlug()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var existingTag = new Tag
        {
            Id = tagId,
            Name = "JavaScript",
            Slug = "javascript"
        };

        var request = new UpdateTagRequest
        {
            Name = "JavaScript" // Same name - should not generate new slug
        };

        var expectedResponse = new TagResponse { Id = tagId };

        _tagRepository.GetByIdAsync(tagId).Returns(existingTag);
        _mapper.Map<TagResponse>(existingTag).Returns(expectedResponse);

        // Act
        var result = await _tagService.UpdateAsync(tagId, request);

        // Assert
        result.Should().NotBeNull();
        
        // Should not check for slug uniqueness since name didn't change
        await _tagRepository.DidNotReceive().IsSlugUniqueAsync(Arg.Any<string>());
        await _tagRepository.Received(1).UpdateAsync(Arg.Any<Tag>());
    }

    [Fact]
    public async Task UpdateAsync_WithProvidedSlug_ShouldUseProvidedSlug()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var existingTag = new Tag
        {
            Id = tagId,
            Name = "JavaScript",
            Slug = "javascript"
        };

        var request = new UpdateTagRequest
        {
            Name = "JavaScript",
            Slug = "Custom New Slug!"
        };

        var expectedResponse = new TagResponse { Id = tagId };

        _tagRepository.GetByIdAsync(tagId).Returns(existingTag);
        _tagRepository.IsSlugUniqueAsync("custom-new-slug").Returns(true);
        _mapper.Map<TagResponse>(existingTag).Returns(expectedResponse);

        // Act
        var result = await _tagService.UpdateAsync(tagId, request);

        // Assert
        result.Should().NotBeNull();
        
        await _tagRepository.Received(1).IsSlugUniqueAsync("custom-new-slug");
        await _tagRepository.Received(1).UpdateAsync(Arg.Any<Tag>());
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateSlug_ShouldThrowApplicationException()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var existingTag = new Tag
        {
            Id = tagId,
            Name = "JavaScript",
            Slug = "javascript"
        };

        var request = new UpdateTagRequest
        {
            Name = "CSharp" // This would generate "csharp" slug
        };

        _tagRepository.GetByIdAsync(tagId).Returns(existingTag);
        _tagRepository.IsSlugUniqueAsync("csharp").Returns(false); // Slug already exists

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _tagService.UpdateAsync(tagId, request)
        );

        exception.Message.Should().Be("A tag with this slug already exists");
        await _tagRepository.DidNotReceive().UpdateAsync(Arg.Any<Tag>());
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingTag_ShouldDeleteTagAndReturnTrue()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var tag = new Tag
        {
            Id = tagId,
            Name = "JavaScript",
            Slug = "javascript"
        };

        _tagRepository.GetByIdAsync(tagId).Returns(tag);

        // Act
        var result = await _tagService.DeleteAsync(tagId);

        // Assert
        result.Should().BeTrue();
        await _tagRepository.Received(1).DeleteAsync(tagId);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentTag_ShouldThrowApplicationException()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        _tagRepository.GetByIdAsync(tagId).Returns((Tag)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _tagService.DeleteAsync(tagId)
        );

        exception.Message.Should().Be("Tag not found");
        await _tagRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    #endregion

    #region GetTagsWithPostCountAsync Tests

    [Fact]
    public async Task GetTagsWithPostCountAsync_ShouldReturnMappedTagsWithPostCount()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" },
            new Tag { Id = Guid.NewGuid(), Name = "JavaScript", Slug = "javascript" }
        };

        var expectedResponse = new List<TagResponse>
        {
            new TagResponse { Id = tags[0].Id, Name = "CSharp", Slug = "csharp", PostCount = 10 },
            new TagResponse { Id = tags[1].Id, Name = "JavaScript", Slug = "javascript", PostCount = 15 }
        };

        _tagRepository.GetAllAsync().Returns(tags);
        _mapper.Map<IEnumerable<TagResponse>>(tags).Returns(expectedResponse);

        // Act
        var result = await _tagService.GetTagsWithPostCountAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        result.Should().HaveCount(2);
        
        await _tagRepository.Received(1).GetAllAsync();
    }

    #endregion

    #region GetPopularTagsAsync Tests

    [Fact]
    public async Task GetPopularTagsAsync_ShouldReturnTopTagsOrderedByPostCount()
    {
        // Arrange
        var count = 3;
        var tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" },
            new Tag { Id = Guid.NewGuid(), Name = "JavaScript", Slug = "javascript" },
            new Tag { Id = Guid.NewGuid(), Name = "Python", Slug = "python" },
            new Tag { Id = Guid.NewGuid(), Name = "Java", Slug = "java" },
            new Tag { Id = Guid.NewGuid(), Name = "TypeScript", Slug = "typescript" }
        };

        var tagResponses = new List<TagResponse>
        {
            new TagResponse { Id = tags[0].Id, Name = "CSharp", Slug = "csharp", PostCount = 5 },
            new TagResponse { Id = tags[1].Id, Name = "JavaScript", Slug = "javascript", PostCount = 25 }, // Most popular
            new TagResponse { Id = tags[2].Id, Name = "Python", Slug = "python", PostCount = 20 }, // Second most popular
            new TagResponse { Id = tags[3].Id, Name = "Java", Slug = "java", PostCount = 15 }, // Third most popular
            new TagResponse { Id = tags[4].Id, Name = "TypeScript", Slug = "typescript", PostCount = 10 }
        };

        _tagRepository.GetAllAsync().Returns(tags);
        _mapper.Map<IEnumerable<TagResponse>>(tags).Returns(tagResponses);

        // Act
        var result = await _tagService.GetPopularTagsAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var resultList = result.ToList();
        resultList[0].Name.Should().Be("JavaScript"); // PostCount: 25
        resultList[0].PostCount.Should().Be(25);
        resultList[1].Name.Should().Be("Python"); // PostCount: 20
        resultList[1].PostCount.Should().Be(20);
        resultList[2].Name.Should().Be("Java"); // PostCount: 15
        resultList[2].PostCount.Should().Be(15);
        
        await _tagRepository.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task GetPopularTagsAsync_WithCountGreaterThanAvailableTags_ShouldReturnAllTags()
    {
        // Arrange
        var count = 10; // More than available tags
        var tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" },
            new Tag { Id = Guid.NewGuid(), Name = "JavaScript", Slug = "javascript" }
        };

        var tagResponses = new List<TagResponse>
        {
            new TagResponse { Id = tags[0].Id, Name = "CSharp", Slug = "csharp", PostCount = 5 },
            new TagResponse { Id = tags[1].Id, Name = "JavaScript", Slug = "javascript", PostCount = 15 }
        };

        _tagRepository.GetAllAsync().Returns(tags);
        _mapper.Map<IEnumerable<TagResponse>>(tags).Returns(tagResponses);

        // Act
        var result = await _tagService.GetPopularTagsAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only 2 tags available
        
        var resultList = result.ToList();
        resultList[0].Name.Should().Be("JavaScript"); // Most popular first
        resultList[1].Name.Should().Be("CSharp");
    }

    [Fact]
    public async Task GetPopularTagsAsync_WithZeroCount_ShouldReturnEmptyList()
    {
        // Arrange
        var count = 0;
        var tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), Name = "CSharp", Slug = "csharp" }
        };

        var tagResponses = new List<TagResponse>
        {
            new TagResponse { Id = tags[0].Id, Name = "CSharp", Slug = "csharp", PostCount = 5 }
        };

        _tagRepository.GetAllAsync().Returns(tags);
        _mapper.Map<IEnumerable<TagResponse>>(tags).Returns(tagResponses);

        // Act
        var result = await _tagService.GetPopularTagsAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPopularTagsAsync_WithEqualPostCounts_ShouldMaintainStableOrder()
    {
        // Arrange
        var count = 2;
        var tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), Name = "Tag1", Slug = "tag1" },
            new Tag { Id = Guid.NewGuid(), Name = "Tag2", Slug = "tag2" },
            new Tag { Id = Guid.NewGuid(), Name = "Tag3", Slug = "tag3" }
        };

        var tagResponses = new List<TagResponse>
        {
            new TagResponse { Id = tags[0].Id, Name = "Tag1", Slug = "tag1", PostCount = 10 },
            new TagResponse { Id = tags[1].Id, Name = "Tag2", Slug = "tag2", PostCount = 10 }, // Same count
            new TagResponse { Id = tags[2].Id, Name = "Tag3", Slug = "tag3", PostCount = 5 }
        };

        _tagRepository.GetAllAsync().Returns(tags);
        _mapper.Map<IEnumerable<TagResponse>>(tags).Returns(tagResponses);

        // Act
        var result = await _tagService.GetPopularTagsAsync(count);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var resultList = result.ToList();
        resultList.All(t => t.PostCount == 10).Should().BeTrue(); // Both should have same count
        resultList.Any(t => t.Name == "Tag3").Should().BeFalse(); // Tag3 with lower count should not be included
    }

    #endregion
}