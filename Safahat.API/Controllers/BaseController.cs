using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Safahat.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the ID of the currently authenticated user
    /// </summary>
    protected Guid UserId => 
        User.Identity?.IsAuthenticated == true && 
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) 
            ? id 
            : Guid.Empty;

    /// <summary>
    /// Gets whether the current user is an administrator
    /// </summary>
    protected bool IsAdmin => 
        User.Identity?.IsAuthenticated == true && 
        User.IsInRole("Admin");
    
    /// <summary>
    /// Checks if the current user is authorised to access a resource
    /// </summary>
    protected bool UserCanAccessResource(Guid resourceOwnerId)
    {
        // Admin can always access
        if (IsAdmin)
            return true;
            
        // For non-admins, they can only access their own resources
        return resourceOwnerId == UserId;
    }
}