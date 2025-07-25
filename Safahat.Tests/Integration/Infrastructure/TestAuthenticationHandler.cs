using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Safahat.Tests.Integration.Infrastructure;

/// <summary>
/// Test authentication handler that creates fake user claims for testing
/// Reads user info from HTTP headers and sets claims that BaseController can use
/// </summary>
public class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string DefaultScheme = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if test headers are present for authentication
        if (!Request.Headers.ContainsKey("X-Test-UserId"))
        {
            // No test auth headers = unauthenticated
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Extract test user info from headers
        var userId = Request.Headers["X-Test-UserId"].ToString();
        var role = Request.Headers.ContainsKey("X-Test-Role") 
            ? Request.Headers["X-Test-Role"].ToString() 
            : "Reader";
        var username = Request.Headers.ContainsKey("X-Test-Username") 
            ? Request.Headers["X-Test-Username"].ToString() 
            : "testuser";
        var email = Request.Headers.ContainsKey("X-Test-Email") 
            ? Request.Headers["X-Test-Email"].ToString() 
            : "test@example.com";

        // Create claims that BaseController expects
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),    // For BaseController.UserId
            new(ClaimTypes.Role, role),                // For BaseController.IsAdmin
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email)
        };

        // Create fake identity and principal
        var identity = new ClaimsIdentity(claims, DefaultScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, DefaultScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}