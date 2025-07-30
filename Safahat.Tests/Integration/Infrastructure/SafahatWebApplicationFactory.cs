using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Safahat.Infrastructure.Data.Context;

namespace Safahat.Tests.Integration.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that configures the test environment
/// - Replaces real database with in-memory database
/// - Replaces real authentication with test authentication
/// - Seeds consistent test data for each test
/// </summary>
public class SafahatWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly object _lock = new object();
    private static int _databaseCounter = 0;

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

            // Create unique database name for each test instance
            string databaseName;
            lock (_lock)
            {
                databaseName = $"TestDatabase_{++_databaseCounter}_{Guid.NewGuid():N}";
            }

            // Add in-memory database for testing
            services.AddDbContext<SafahatDbContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging(); // Helpful for debugging tests
            });

            // Replace real authentication with test authentication
            services.AddAuthentication(TestAuthenticationHandler.DefaultScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.DefaultScheme, _ => { });
        });
    }

    /// <summary>
    /// Creates an HTTP client with no authentication (for testing public endpoints)
    /// Ensures fresh database state before returning the client
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        EnsureFreshDatabase();
        return CreateClient();
    }

    /// <summary>
    /// Creates an HTTP client authenticated as a reader user
    /// Ensures fresh database state before returning the client
    /// </summary>
    public HttpClient CreateReaderClient()
    {
        EnsureFreshDatabase();
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", TestDataSeeder.ReaderUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Reader");
        client.DefaultRequestHeaders.Add("X-Test-Username", "readeruser");
        client.DefaultRequestHeaders.Add("X-Test-Email", "reader@test.com");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client authenticated as an author user
    /// Ensures fresh database state before returning the client
    /// </summary>
    public HttpClient CreateAuthorClient()
    {
        EnsureFreshDatabase();
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", TestDataSeeder.AuthorUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Author");
        client.DefaultRequestHeaders.Add("X-Test-Username", "authoruser");
        client.DefaultRequestHeaders.Add("X-Test-Email", "author@test.com");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client authenticated as an admin user
    /// Ensures fresh database state before returning the client
    /// </summary>
    public HttpClient CreateAdminClient()
    {
        EnsureFreshDatabase();
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", TestDataSeeder.AdminUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        client.DefaultRequestHeaders.Add("X-Test-Username", "adminuser");
        client.DefaultRequestHeaders.Add("X-Test-Email", "admin@test.com");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client authenticated as another reader user (for testing permissions)
    /// Ensures fresh database state before returning the client
    /// </summary>
    public HttpClient CreateOtherReaderClient()
    {
        EnsureFreshDatabase();
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", TestDataSeeder.OtherReaderUserId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", "Reader");
        client.DefaultRequestHeaders.Add("X-Test-Username", "otherreader");
        client.DefaultRequestHeaders.Add("X-Test-Email", "other@test.com");
        return client;
    }

    /// <summary>
    /// Creates an HTTP client authenticated as a custom user
    /// Ensures fresh database state before returning the client
    /// </summary>
    public HttpClient CreateAuthenticatedClient(Guid userId, string role = "Reader", string username = "testuser", string email = "test@example.com")
    {
        EnsureFreshDatabase();
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Add("X-Test-Username", username);
        client.DefaultRequestHeaders.Add("X-Test-Email", email);
        return client;
    }

    /// <summary>
    /// Ensures the database is in a fresh state with clean test data
    /// This method is called before each client creation to ensure test isolation
    /// </summary>
    private void EnsureFreshDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SafahatDbContext>();
        
        // Ensure clean state - delete and recreate database
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        // Seed fresh test data
        TestDataSeeder.SeedData(context);
    }

    /// <summary>
    /// Alternative method for tests that need multiple clients but want to share the same database state
    /// Call this once at the beginning of a test, then create multiple clients without re-seeding
    /// </summary>
    public void ResetDatabase()
    {
        EnsureFreshDatabase();
    }

    /// <summary>
    /// Creates a client without resetting the database (for tests that need multiple clients with shared state)
    /// Use ResetDatabase() first, then call this method for subsequent clients in the same test
    /// </summary>
    public HttpClient CreateClientWithoutReset(Guid userId, string role, string username, string email)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Add("X-Test-Username", username);
        client.DefaultRequestHeaders.Add("X-Test-Email", email);
        return client;
    }
}