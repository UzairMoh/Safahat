using Safahat.Infrastructure.Data.Context;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Tests.Integration.Infrastructure;

/// <summary>
/// Seeds the test database with consistent test data for all integration tests.
/// Provides known entity IDs and realistic test scenarios for comprehensive testing.
/// </summary>
public static class TestDataSeeder
{
    #region Test Entity IDs
    
    // Test users with descriptive names
    public static readonly Guid ReaderUserId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AuthorUserId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AdminUserId = new("33333333-3333-3333-3333-333333333333");
    public static readonly Guid OtherReaderUserId = new("44444444-4444-4444-4444-444444444444");
    public static readonly Guid InactiveUserId = new("55555555-5555-5555-5555-555555555555");
    
    // Test categories
    public static readonly Guid TechnologyCategoryId = new("66666666-6666-6666-6666-666666666666");
    public static readonly Guid LifestyleCategoryId = new("77777777-7777-7777-7777-777777777777");
    
    // Test tags
    public static readonly Guid CSharpTagId = new("88888888-8888-8888-8888-888888888888");
    public static readonly Guid TestingTagId = new("99999999-9999-9999-9999-999999999999");
    
    // Test posts
    public static readonly Guid PublishedPostId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid DraftPostId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid FeaturedPostId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid AuthorPostId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
    
    // Test comments
    public static readonly Guid ApprovedCommentId = new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    public static readonly Guid PendingCommentId = new("ffffffff-ffff-ffff-ffff-ffffffffffff");
    
    #endregion

    #region Constants for Test Data
    
    private static readonly DateTime BaseDateTime = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    
    #endregion

    /// <summary>
    /// Seeds the test database with all necessary test data.
    /// Ensures referential integrity and realistic data relationships.
    /// </summary>
    /// <param name="context">The database context to seed</param>
    public static void SeedData(SafahatDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        // Ensure clean state
        ResetDatabase(context);
        
        // Seed in order to maintain referential integrity
        SeedUsers(context);
        SeedCategories(context);
        SeedTags(context);
        SeedPosts(context);
        SeedPostRelationships(context);
        SeedComments(context);
        
        context.SaveChanges();
    }

    #region Private Seeding Methods
    
    private static void ResetDatabase(SafahatDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
    
    private static void SeedUsers(SafahatDbContext context)
    {
        var users = new User[]
        {
            CreateUser(
                id: ReaderUserId,
                username: "readeruser",
                email: "reader@test.com",
                firstName: "Reader",
                lastName: "User",
                role: UserRole.Reader,
                isActive: true,
                daysAgo: 30
            ),
            CreateUser(
                id: AuthorUserId,
                username: "authoruser",
                email: "author@test.com",
                firstName: "Author",
                lastName: "User",
                role: UserRole.Author,
                isActive: true,
                daysAgo: 45
            ),
            CreateUser(
                id: AdminUserId,
                username: "adminuser",
                email: "admin@test.com",
                firstName: "Admin",
                lastName: "User",
                role: UserRole.Admin,
                isActive: true,
                daysAgo: 60
            ),
            CreateUser(
                id: OtherReaderUserId,
                username: "otherreader",
                email: "other@test.com",
                firstName: "Other",
                lastName: "Reader",
                role: UserRole.Reader,
                isActive: true,
                daysAgo: 15
            ),
            CreateUser(
                id: InactiveUserId,
                username: "inactiveuser",
                email: "inactive@test.com",
                firstName: "Inactive",
                lastName: "User",
                role: UserRole.Reader,
                isActive: false,
                daysAgo: 90
            )
        };
        
        context.Users.AddRange(users);
    }
    
    private static void SeedCategories(SafahatDbContext context)
    {
        var categories = new Category[]
        {
            CreateCategory(
                id: TechnologyCategoryId,
                name: "Technology",
                slug: "technology",
                description: "Technology and programming related posts",
                daysAgo: 20
            ),
            CreateCategory(
                id: LifestyleCategoryId,
                name: "Lifestyle",
                slug: "lifestyle",
                description: "Lifestyle and personal development posts",
                daysAgo: 20
            )
        };
        
        context.Categories.AddRange(categories);
    }
    
    private static void SeedTags(SafahatDbContext context)
    {
        var tags = new Tag[]
        {
            CreateTag(
                id: CSharpTagId,
                name: "csharp",
                slug: "csharp",
                daysAgo: 15
            ),
            CreateTag(
                id: TestingTagId,
                name: "testing",
                slug: "testing",
                daysAgo: 15
            )
        };
        
        context.Tags.AddRange(tags);
    }
    
    private static void SeedPosts(SafahatDbContext context)
    {
        var posts = new Post[]
        {
            CreatePost(
                id: PublishedPostId,
                title: "Getting Started with C# Testing",
                content: "This comprehensive guide covers the fundamentals of testing in C#, including unit tests, integration tests, and best practices for maintaining high-quality code.",
                summary: "A comprehensive guide to C# testing fundamentals",
                slug: "getting-started-csharp-testing",
                status: PostStatus.Published,
                authorId: ReaderUserId,
                viewCount: 125,
                isFeatured: false,
                publishedDaysAgo: 5,
                createdDaysAgo: 7
            ),
            CreatePost(
                id: DraftPostId,
                title: "Advanced Integration Testing Techniques",
                content: "This post explores advanced techniques for integration testing in modern web applications, including database testing, authentication testing, and performance considerations.",
                summary: "Advanced integration testing strategies and techniques",
                slug: "advanced-integration-testing-techniques",
                status: PostStatus.Draft,
                authorId: ReaderUserId,
                viewCount: 0,
                isFeatured: false,
                publishedDaysAgo: null,
                createdDaysAgo: 3
            ),
            CreatePost(
                id: FeaturedPostId,
                title: "The Future of Software Development",
                content: "An in-depth analysis of emerging trends in software development, including AI-assisted coding, cloud-native architectures, and the evolution of programming languages.",
                summary: "Exploring emerging trends in software development",
                slug: "future-of-software-development",
                status: PostStatus.Published,
                authorId: AdminUserId,
                viewCount: 450,
                isFeatured: true,
                publishedDaysAgo: 10,
                createdDaysAgo: 12
            ),
            CreatePost(
                id: AuthorPostId,
                title: "Personal Productivity Tips for Developers",
                content: "Practical tips and strategies for improving personal productivity as a software developer, including time management, tool selection, and work-life balance.",
                summary: "Productivity tips specifically for software developers",
                slug: "productivity-tips-for-developers",
                status: PostStatus.Published,
                authorId: AuthorUserId,
                viewCount: 89,
                isFeatured: false,
                publishedDaysAgo: 8,
                createdDaysAgo: 9
            )
        };
        
        context.Posts.AddRange(posts);
    }
    
    private static void SeedPostRelationships(SafahatDbContext context)
    {
        // Post-Category relationships
        var postCategories = new PostCategory[]
        {
            new() { PostId = PublishedPostId, CategoryId = TechnologyCategoryId },
            new() { PostId = DraftPostId, CategoryId = TechnologyCategoryId },
            new() { PostId = FeaturedPostId, CategoryId = TechnologyCategoryId },
            new() { PostId = AuthorPostId, CategoryId = LifestyleCategoryId }
        };
        
        // Post-Tag relationships
        var postTags = new PostTag[]
        {
            new() { PostId = PublishedPostId, TagId = CSharpTagId },
            new() { PostId = PublishedPostId, TagId = TestingTagId },
            new() { PostId = DraftPostId, TagId = TestingTagId },
            new() { PostId = FeaturedPostId, TagId = CSharpTagId },
            new() { PostId = AuthorPostId, TagId = TestingTagId }
        };
        
        context.PostCategories.AddRange(postCategories);
        context.PostTags.AddRange(postTags);
    }
    
    private static void SeedComments(SafahatDbContext context)
    {
        var comments = new Comment[]
        {
            CreateComment(
                id: ApprovedCommentId,
                content: "Excellent article! The examples you provided really helped me understand the concepts better. Looking forward to more content like this.",
                postId: PublishedPostId,
                userId: OtherReaderUserId,
                isApproved: true,
                daysAgo: 2
            ),
            CreateComment(
                id: PendingCommentId,
                content: "Thanks for sharing this comprehensive guide. The section on best practices was particularly insightful.",
                postId: PublishedPostId,
                userId: AdminUserId,
                isApproved: true,
                daysAgo: 1
            ),
            CreateComment(
                id: Guid.NewGuid(),
                content: "This comment is pending approval and should not appear in public views.",
                postId: FeaturedPostId,
                userId: ReaderUserId,
                isApproved: false,
                daysAgo: 1
            ),
            // Reply to the first comment (hierarchical comment)
            CreateReplyComment(
                id: Guid.NewGuid(),
                content: "I completely agree! This was very helpful for my current project.",
                postId: PublishedPostId,
                userId: AuthorUserId,
                parentCommentId: ApprovedCommentId,
                isApproved: true,
                daysAgo: 1
            )
        };
        
        context.Comments.AddRange(comments);
    }
    
    #endregion

    #region Factory Methods
    
    private static User CreateUser(
        Guid id,
        string username,
        string email,
        string firstName,
        string lastName,
        UserRole role,
        bool isActive,
        int daysAgo)
    {
        var createdAt = BaseDateTime.AddDays(-daysAgo);
        
        return new User
        {
            Id = id,
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = GenerateTestPasswordHash(),
            Role = role,
            IsActive = isActive,
            Bio = $"Test bio for {firstName} {lastName}",
            ProfilePictureUrl = null,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            LastLoginAt = isActive ? createdAt.AddDays(daysAgo / 2) : null
        };
    }
    
    private static Category CreateCategory(Guid id, string name, string slug, string description, int daysAgo)
    {
        var createdAt = BaseDateTime.AddDays(-daysAgo);
        
        return new Category
        {
            Id = id,
            Name = name,
            Slug = slug,
            Description = description,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
    
    private static Tag CreateTag(Guid id, string name, string slug, int daysAgo)
    {
        var createdAt = BaseDateTime.AddDays(-daysAgo);
        
        return new Tag
        {
            Id = id,
            Name = name,
            Slug = slug,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
    
    private static Post CreatePost(
        Guid id,
        string title,
        string content,
        string summary,
        string slug,
        PostStatus status,
        Guid authorId,
        int viewCount,
        bool isFeatured,
        int? publishedDaysAgo,
        int createdDaysAgo)
    {
        var createdAt = BaseDateTime.AddDays(-createdDaysAgo);
        var publishedAt = publishedDaysAgo.HasValue ? BaseDateTime.AddDays(-publishedDaysAgo.Value) : (DateTime?)null;
        var updatedAt = publishedAt ?? createdAt;
        
        return new Post
        {
            Id = id,
            Title = title,
            Content = content,
            Summary = summary,
            Slug = slug,
            Status = status,
            AuthorId = authorId,
            ViewCount = viewCount,
            IsFeatured = isFeatured,
            AllowComments = true,
            FeaturedImageUrl = null,
            PublishedAt = publishedAt,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
    
    private static Comment CreateComment(
        Guid id,
        string content,
        Guid postId,
        Guid userId,
        bool isApproved,
        int daysAgo)
    {
        var createdAt = BaseDateTime.AddDays(-daysAgo);
        
        return new Comment
        {
            Id = id,
            Content = content,
            PostId = postId,
            UserId = userId,
            IsApproved = isApproved,
            ParentCommentId = null, // All test comments are top-level
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
    
    private static Comment CreateReplyComment(
        Guid id,
        string content,
        Guid postId,
        Guid userId,
        Guid parentCommentId,
        bool isApproved,
        int daysAgo)
    {
        var createdAt = BaseDateTime.AddDays(-daysAgo);
        
        return new Comment
        {
            Id = id,
            Content = content,
            PostId = postId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            IsApproved = isApproved,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
    
    private static string GenerateTestPasswordHash()
    {
        // Consistent fake hash for all test users - not used in integration tests anyway
        return "test-password-hash-not-used-in-integration-tests";
    }
    
    #endregion
}