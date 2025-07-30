using System.Security.Cryptography;
using Safahat.Infrastructure.Data.Context;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Tests.Integration.Infrastructure;

/// <summary>
/// Seeds the test database with consistent test data for integration tests.
/// </summary>
public static class TestDataSeeder
{
    #region Test Entity IDs
    
    public static readonly Guid ReaderUserId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid AuthorUserId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AdminUserId = new("33333333-3333-3333-3333-333333333333");
    public static readonly Guid OtherReaderUserId = new("44444444-4444-4444-4444-444444444444");
    public static readonly Guid InactiveUserId = new("55555555-5555-5555-5555-555555555555");
    
    public static readonly Guid TechnologyCategoryId = new("66666666-6666-6666-6666-666666666666");
    public static readonly Guid LifestyleCategoryId = new("77777777-7777-7777-7777-777777777777");
    
    public static readonly Guid CSharpTagId = new("88888888-8888-8888-8888-888888888888");
    public static readonly Guid TestingTagId = new("99999999-9999-9999-9999-999999999999");
    
    public static readonly Guid PublishedPostId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid DraftPostId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid FeaturedPostId = new("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid AuthorPostId = new("dddddddd-dddd-dddd-dddd-dddddddddddd");
    
    public static readonly Guid ApprovedCommentId = new("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    public static readonly Guid PendingCommentId = new("ffffffff-ffff-ffff-ffff-ffffffffffff");
    
    #endregion

    #region Constants
    
    private static readonly DateTime BaseDateTime = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    public const string TestPassword = "test-password";
    
    #endregion

    /// <summary>
    /// Seeds the test database with all necessary test data.
    /// </summary>
    public static void SeedData(SafahatDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        ResetDatabase(context);
        
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
            CreateUser(ReaderUserId, "readeruser", "reader@test.com", "Reader", "User", UserRole.Reader, true, 30),
            CreateUser(AuthorUserId, "authoruser", "author@test.com", "Author", "User", UserRole.Author, true, 45),
            CreateUser(AdminUserId, "adminuser", "admin@test.com", "Admin", "User", UserRole.Admin, true, 60),
            CreateUser(OtherReaderUserId, "otherreader", "other@test.com", "Other", "Reader", UserRole.Reader, true, 15),
            CreateUser(InactiveUserId, "inactiveuser", "inactive@test.com", "Inactive", "User", UserRole.Reader, false, 90)
        };
        
        context.Users.AddRange(users);
    }
    
    private static void SeedCategories(SafahatDbContext context)
    {
        var categories = new Category[]
        {
            CreateCategory(TechnologyCategoryId, "Technology", "technology", "Technology and programming related posts", 20),
            CreateCategory(LifestyleCategoryId, "Lifestyle", "lifestyle", "Lifestyle and personal development posts", 20)
        };
        
        context.Categories.AddRange(categories);
    }
    
    private static void SeedTags(SafahatDbContext context)
    {
        var tags = new Tag[]
        {
            CreateTag(CSharpTagId, "csharp", "csharp", 15),
            CreateTag(TestingTagId, "testing", "testing", 15)
        };
        
        context.Tags.AddRange(tags);
    }
    
    private static void SeedPosts(SafahatDbContext context)
    {
        var posts = new Post[]
        {
            CreatePost(PublishedPostId, "Getting Started with C# Testing", 
                "This comprehensive guide covers the fundamentals of testing in C#, including unit tests, integration tests, and best practices for maintaining high-quality code.",
                "A comprehensive guide to C# testing fundamentals", "getting-started-csharp-testing", 
                PostStatus.Published, ReaderUserId, 125, false, 5, 7),
            
            CreatePost(DraftPostId, "Advanced Integration Testing Techniques",
                "This post explores advanced techniques for integration testing in modern web applications, including database testing, authentication testing, and performance considerations.",
                "Advanced integration testing strategies and techniques", "advanced-integration-testing-techniques",
                PostStatus.Draft, ReaderUserId, 0, false, null, 3),
            
            CreatePost(FeaturedPostId, "The Future of Software Development",
                "An in-depth analysis of emerging trends in software development, including AI-assisted coding, cloud-native architectures, and the evolution of programming languages.",
                "Exploring emerging trends in software development", "future-of-software-development",
                PostStatus.Published, AdminUserId, 450, true, 10, 12),
            
            CreatePost(AuthorPostId, "Personal Productivity Tips for Developers",
                "Practical tips and strategies for improving personal productivity as a software developer, including time management, tool selection, and work-life balance.",
                "Productivity tips specifically for software developers", "productivity-tips-for-developers",
                PostStatus.Published, AuthorUserId, 89, false, 8, 9)
        };
        
        context.Posts.AddRange(posts);
    }
    
    private static void SeedPostRelationships(SafahatDbContext context)
    {
        var postCategories = new PostCategory[]
        {
            new() { PostId = PublishedPostId, CategoryId = TechnologyCategoryId },
            new() { PostId = DraftPostId, CategoryId = TechnologyCategoryId },
            new() { PostId = FeaturedPostId, CategoryId = TechnologyCategoryId },
            new() { PostId = AuthorPostId, CategoryId = LifestyleCategoryId }
        };
        
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
            CreateComment(ApprovedCommentId, 
                "Excellent article! The examples you provided really helped me understand the concepts better. Looking forward to more content like this.",
                PublishedPostId, OtherReaderUserId, 2),
            
            CreateComment(PendingCommentId,
                "Thanks for sharing this comprehensive guide. The section on best practices was particularly insightful.",
                PublishedPostId, AdminUserId, 1),
            
            CreateComment(Guid.NewGuid(),
                "This comment is pending approval and should not appear in public views.",
                FeaturedPostId, ReaderUserId, 1),
            
            CreateReplyComment(Guid.NewGuid(),
                "I completely agree! This was very helpful for my current project.",
                PublishedPostId, AuthorUserId, ApprovedCommentId, 1)
        };
        
        context.Comments.AddRange(comments);
    }
    
    #endregion

    #region Factory Methods
    
    private static User CreateUser(Guid id, string username, string email, string firstName, string lastName, 
        UserRole role, bool isActive, int daysAgo)
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
    
    private static Post CreatePost(Guid id, string title, string content, string summary, string slug,
        PostStatus status, Guid authorId, int viewCount, bool isFeatured, int? publishedDaysAgo, int createdDaysAgo)
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
    
    private static Comment CreateComment(Guid id, string content, Guid postId, Guid userId, int daysAgo)
    {
        var createdAt = BaseDateTime.AddDays(-daysAgo);
        
        return new Comment
        {
            Id = id,
            Content = content,
            PostId = postId,
            UserId = userId,
            ParentCommentId = null,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
    
    private static Comment CreateReplyComment(Guid id, string content, Guid postId, Guid userId, 
        Guid parentCommentId, int daysAgo)
    {
        var createdAt = BaseDateTime.AddDays(-daysAgo);
        
        return new Comment
        {
            Id = id,
            Content = content,
            PostId = postId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
    
    /// <summary>
    /// Generates a properly hashed password that matches the AuthService's hashing algorithm.
    /// </summary>
    private static string GenerateTestPasswordHash()
    {
        // Generate a consistent salt for testing
        byte[] salt = new byte[16];
        for (int i = 0; i < salt.Length; i++)
        {
            salt[i] = (byte)(i + 1);
        }

        // Hash the password with the salt using the same method as AuthService
        using (var pbkdf2 = new Rfc2898DeriveBytes(TestPassword, salt, 10000, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(32);

            // Combine the salt and hash
            byte[] hashWithSalt = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashWithSalt, 0, salt.Length);
            Array.Copy(hash, 0, hashWithSalt, salt.Length, hash.Length);

            return Convert.ToBase64String(hashWithSalt);
        }
    }
    
    #endregion
}