using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Safahat.Application.DTOs.Requests.Categories;
using Safahat.Application.DTOs.Responses.Categories;
using Safahat.Application.Services;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Tests.Services;

public class CategoryServiceTests
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _mapper = Substitute.For<IMapper>();
        
        _categoryService = new CategoryService(
            _categoryRepository,
            _mapper
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingCategory_ShouldReturnMappedCategoryResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Technology",
            Slug = "technology",
            Description = "Tech articles"
        };

        var expectedResponse = new CategoryResponse
        {
            Id = categoryId,
            Name = "Technology",
            Slug = "technology",
            Description = "Tech articles"
        };

        _categoryRepository.GetByIdAsync(categoryId).Returns(category);
        _mapper.Map<CategoryResponse>(category).Returns(expectedResponse);

        // Act
        var result = await _categoryService.GetByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _categoryRepository.Received(1).GetByIdAsync(categoryId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentCategory_ShouldThrowApplicationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId).Returns((Category)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _categoryService.GetByIdAsync(categoryId)
        );

        exception.Message.Should().Be("Category not found");
    }

    #endregion

    #region GetBySlugAsync Tests

    [Fact]
    public async Task GetBySlugAsync_WithExistingSlug_ShouldReturnMappedCategoryResponse()
    {
        // Arrange
        var slug = "technology";
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Technology",
            Slug = slug,
            Description = "Tech articles"
        };

        var expectedResponse = new CategoryResponse
        {
            Id = category.Id,
            Name = "Technology",
            Slug = slug,
            Description = "Tech articles"
        };

        _categoryRepository.GetBySlugAsync(slug).Returns(category);
        _mapper.Map<CategoryResponse>(category).Returns(expectedResponse);

        // Act
        var result = await _categoryService.GetBySlugAsync(slug);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _categoryRepository.Received(1).GetBySlugAsync(slug);
    }

    [Fact]
    public async Task GetBySlugAsync_WithNonExistentSlug_ShouldThrowApplicationException()
    {
        // Arrange
        var slug = "non-existent";
        _categoryRepository.GetBySlugAsync(slug).Returns((Category)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _categoryService.GetBySlugAsync(slug)
        );

        exception.Message.Should().Be("Category not found");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnMappedCategoriesList()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Technology", Slug = "technology" },
            new Category { Id = Guid.NewGuid(), Name = "Sports", Slug = "sports" }
        };

        var expectedResponse = new List<CategoryResponse>
        {
            new CategoryResponse { Id = categories[0].Id, Name = "Technology", Slug = "technology" },
            new CategoryResponse { Id = categories[1].Id, Name = "Sports", Slug = "sports" }
        };

        _categoryRepository.GetAllAsync().Returns(categories);
        _mapper.Map<IEnumerable<CategoryResponse>>(categories).Returns(expectedResponse);

        // Act
        var result = await _categoryService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        await _categoryRepository.Received(1).GetAllAsync();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDataAndNoSlug_ShouldGenerateSlugAndCreateCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Technology & Innovation",
            Description = "Latest tech trends"
        };

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = "technology-innovation", // Generated slug
            Description = request.Description
        };

        var expectedResponse = new CategoryResponse
        {
            Id = category.Id,
            Name = request.Name,
            Slug = "technology-innovation",
            Description = request.Description
        };

        _categoryRepository.IsSlugUniqueAsync("technology-innovation").Returns(true);
        _mapper.Map<Category>(Arg.Is<CreateCategoryRequest>(r => r.Slug == "technology-innovation")).Returns(category);
        _categoryRepository.AddAsync(category).Returns(category);
        _mapper.Map<CategoryResponse>(category).Returns(expectedResponse);

        // Act
        var result = await _categoryService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        result.Slug.Should().Be("technology-innovation");
        
        await _categoryRepository.Received(1).IsSlugUniqueAsync("technology-innovation");
        await _categoryRepository.Received(1).AddAsync(category);
    }

    [Fact]
    public async Task CreateAsync_WithProvidedSlug_ShouldNormalizeSlugAndCreateCategory()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Technology",
            Slug = "Custom SLUG with Spaces!",
            Description = "Tech articles"
        };

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = "custom-slug-with-spaces", // Normalized slug
            Description = request.Description
        };

        var expectedResponse = new CategoryResponse
        {
            Id = category.Id,
            Name = request.Name,
            Slug = "custom-slug-with-spaces",
            Description = request.Description
        };

        _categoryRepository.IsSlugUniqueAsync("custom-slug-with-spaces").Returns(true);
        _mapper.Map<Category>(Arg.Is<CreateCategoryRequest>(r => r.Slug == "custom-slug-with-spaces")).Returns(category);
        _categoryRepository.AddAsync(category).Returns(category);
        _mapper.Map<CategoryResponse>(category).Returns(expectedResponse);

        // Act
        var result = await _categoryService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Slug.Should().Be("custom-slug-with-spaces");
        
        await _categoryRepository.Received(1).IsSlugUniqueAsync("custom-slug-with-spaces");
        await _categoryRepository.Received(1).AddAsync(category);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSlug_ShouldThrowApplicationException()
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = "Technology",
            Description = "Tech articles"
        };

        _categoryRepository.IsSlugUniqueAsync("technology").Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _categoryService.CreateAsync(request)
        );

        exception.Message.Should().Be("A category with this slug already exists");
        await _categoryRepository.DidNotReceive().AddAsync(Arg.Any<Category>());
    }

    [Theory]
    [InlineData("Technology & Innovation", "technology-innovation")]
    [InlineData("Café & Restaurant", "cafe-restaurant")]
    [InlineData("Sports!!!", "sports")]
    [InlineData("   Multiple   Spaces   ", "multiple-spaces")]
    [InlineData("UPPERCASE TEXT", "uppercase-text")]
    [InlineData("Special@#$%Characters", "specialcharacters")]
    [InlineData("Açaí & Müsli", "acai-musli")]
    public async Task CreateAsync_SlugGeneration_ShouldCreateCorrectSlugs(string inputName, string expectedSlug)
    {
        // Arrange
        var request = new CreateCategoryRequest
        {
            Name = inputName,
            Description = "Test category"
        };

        var category = new Category { Id = Guid.NewGuid() };
        var response = new CategoryResponse { Id = category.Id };

        _categoryRepository.IsSlugUniqueAsync(expectedSlug).Returns(true);
        _mapper.Map<Category>(Arg.Is<CreateCategoryRequest>(r => r.Slug == expectedSlug)).Returns(category);
        _categoryRepository.AddAsync(category).Returns(category);
        _mapper.Map<CategoryResponse>(category).Returns(response);

        // Act
        await _categoryService.CreateAsync(request);

        // Assert
        await _categoryRepository.Received(1).IsSlugUniqueAsync(expectedSlug);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateCategoryAndReturnResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Technology",
            Slug = "technology",
            Description = "Old description"
        };

        var request = new UpdateCategoryRequest
        {
            Name = "Advanced Technology",
            Description = "Updated description"
        };

        var expectedResponse = new CategoryResponse
        {
            Id = categoryId,
            Name = "Advanced Technology",
            Slug = "advanced-technology",
            Description = "Updated description"
        };

        _categoryRepository.GetByIdAsync(categoryId).Returns(existingCategory);
        _categoryRepository.IsSlugUniqueAsync("advanced-technology").Returns(true);
        _mapper.Map<CategoryResponse>(existingCategory).Returns(expectedResponse);

        // Act
        var result = await _categoryService.UpdateAsync(categoryId, request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        
        await _categoryRepository.Received(1).UpdateAsync(Arg.Is<Category>(c => 
            c.Id == categoryId && 
            c.UpdatedAt != null));
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentCategory_ShouldThrowApplicationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = new UpdateCategoryRequest { Name = "Updated Name" };

        _categoryRepository.GetByIdAsync(categoryId).Returns((Category)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _categoryService.UpdateAsync(categoryId, request)
        );

        exception.Message.Should().Be("Category not found");
        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task UpdateAsync_WithSameName_ShouldNotGenerateNewSlug()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Technology",
            Slug = "technology",
            Description = "Old description"
        };

        var request = new UpdateCategoryRequest
        {
            Name = "Technology", // Same name
            Description = "Updated description"
        };

        var expectedResponse = new CategoryResponse { Id = categoryId };

        _categoryRepository.GetByIdAsync(categoryId).Returns(existingCategory);
        _mapper.Map<CategoryResponse>(existingCategory).Returns(expectedResponse);

        // Act
        var result = await _categoryService.UpdateAsync(categoryId, request);

        // Assert
        result.Should().NotBeNull();
        
        // Should not check for slug uniqueness since name didn't change
        await _categoryRepository.DidNotReceive().IsSlugUniqueAsync(Arg.Any<string>());
        await _categoryRepository.Received(1).UpdateAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task UpdateAsync_WithProvidedSlug_ShouldUseProvidedSlug()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Technology",
            Slug = "technology",
            Description = "Old description"
        };

        var request = new UpdateCategoryRequest
        {
            Name = "Technology",
            Slug = "Custom New Slug!",
            Description = "Updated description"
        };

        var expectedResponse = new CategoryResponse { Id = categoryId };

        _categoryRepository.GetByIdAsync(categoryId).Returns(existingCategory);
        _categoryRepository.IsSlugUniqueAsync("custom-new-slug").Returns(true);
        _mapper.Map<CategoryResponse>(existingCategory).Returns(expectedResponse);

        // Act
        var result = await _categoryService.UpdateAsync(categoryId, request);

        // Assert
        result.Should().NotBeNull();
        
        await _categoryRepository.Received(1).IsSlugUniqueAsync("custom-new-slug");
        await _categoryRepository.Received(1).UpdateAsync(Arg.Any<Category>());
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateSlug_ShouldThrowApplicationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existingCategory = new Category
        {
            Id = categoryId,
            Name = "Technology",
            Slug = "technology",
            Description = "Old description"
        };

        var request = new UpdateCategoryRequest
        {
            Name = "Sports", // This would generate "sports" slug
            Description = "Updated description"
        };

        _categoryRepository.GetByIdAsync(categoryId).Returns(existingCategory);
        _categoryRepository.IsSlugUniqueAsync("sports").Returns(false); // Slug already exists

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _categoryService.UpdateAsync(categoryId, request)
        );

        exception.Message.Should().Be("A category with this slug already exists");
        await _categoryRepository.DidNotReceive().UpdateAsync(Arg.Any<Category>());
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingCategory_ShouldDeleteCategoryAndReturnTrue()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            Name = "Technology",
            Slug = "technology"
        };

        _categoryRepository.GetByIdAsync(categoryId).Returns(category);

        // Act
        var result = await _categoryService.DeleteAsync(categoryId);

        // Assert
        result.Should().BeTrue();
        await _categoryRepository.Received(1).DeleteAsync(categoryId);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentCategory_ShouldThrowApplicationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(categoryId).Returns((Category)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _categoryService.DeleteAsync(categoryId)
        );

        exception.Message.Should().Be("Category not found");
        await _categoryRepository.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    #endregion

    #region GetCategoriesWithPostCountAsync Tests

    [Fact]
    public async Task GetCategoriesWithPostCountAsync_ShouldReturnMappedCategoriesWithPostCount()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Technology", Slug = "technology" },
            new Category { Id = Guid.NewGuid(), Name = "Sports", Slug = "sports" }
        };

        var expectedResponse = new List<CategoryResponse>
        {
            new CategoryResponse { Id = categories[0].Id, Name = "Technology", Slug = "technology", PostCount = 5 },
            new CategoryResponse { Id = categories[1].Id, Name = "Sports", Slug = "sports", PostCount = 3 }
        };

        _categoryRepository.GetAllAsync().Returns(categories);
        _mapper.Map<IEnumerable<CategoryResponse>>(categories).Returns(expectedResponse);

        // Act
        var result = await _categoryService.GetCategoriesWithPostCountAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        result.Should().HaveCount(2);
        
        await _categoryRepository.Received(1).GetAllAsync();
    }

    #endregion
}