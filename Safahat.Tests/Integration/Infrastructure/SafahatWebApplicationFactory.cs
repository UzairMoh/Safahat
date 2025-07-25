using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Safahat.Infrastructure.Data.Context;

namespace Safahat.Tests.Integration.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that configures the test environment
/// - Replaces real database with in-memory database
/// - Replaces real authentication with test authentication
/// - Seeds consistent test data
/// </summary>
public class SafahatWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use testing environment first
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Remove all database-related registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<SafahatDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var dbContextOptionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions));
            if (dbContextOptionsDescriptor != null)
                services.Remove(dbContextOptionsDescriptor);

            // Remove any PostgreSQL-specific services
            var descriptorsToRemove = services.Where(d => 
                d.ServiceType.ToString().Contains("Npgsql") ||
                d.ServiceType.ToString().Contains("PostgreSQL") ||
                d.ImplementationType?.ToString().Contains("Npgsql") == true ||
                d.ImplementationType?.ToString().Contains("PostgreSQL") == true)
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<SafahatDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase");
                options.EnableSensitiveDataLogging(); // Helpful for debugging tests
            });

            // Replace real authentication with test authentication
            services.AddAuthentication(TestAuthenticationHandler.DefaultScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.DefaultScheme, _ => { });

            // Build service provider and seed the database
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SafahatDbContext>();
            
            // Seed test data
            TestDataSeeder.SeedData(context);
        });
    }

    /// <summary>
    /// Creates an HTTP client with no authentication (for testing public endpoints)
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        return CreateClient();
    }

    /// <summary>
    /// Creates an HTTP client authenticated as a reader user
    /// </summary>
    public HttpClient CreateReaderClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", TestDataSeeder.ReaderUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Reader");
        client.DefaultRequestHeaders.Add("X-Test-Username", "readeruser");
        client.DefaultRequestHeaders.Add("X-Test-Email", "reader@test.com");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client authenticated as an author user
    /// </summary>
    public HttpClient CreateAuthorClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", TestDataSeeder.AuthorUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Author");
        client.DefaultRequestHeaders.Add("X-Test-Username", "authoruser");
        client.DefaultRequestHeaders.Add("X-Test-Email", "author@test.com");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client authenticated as an admin user
    /// </summary>
    public HttpClient CreateAdminClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", TestDataSeeder.AdminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        client.DefaultRequestHeaders.Add("X-Test-Username", "adminuser");
        client.DefaultRequestHeaders.Add("X-Test-Email", "admin@test.com");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client authenticated as another reader user (for testing permissions)
    /// </summary>
    public HttpClient CreateOtherReaderClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", TestDataSeeder.OtherReaderUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Reader");
        client.DefaultRequestHeaders.Add("X-Test-Username", "otherreader");
        client.DefaultRequestHeaders.Add("X-Test-Email", "other@test.com");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client authenticated as a custom user
    /// </summary>
    public HttpClient CreateAuthenticatedClient(Guid userId, string role = "Reader", string username = "testuser", string email = "test@example.com")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Add("X-Test-Username", username);
        client.DefaultRequestHeaders.Add("X-Test-Email", email);
        return client;
    }
}