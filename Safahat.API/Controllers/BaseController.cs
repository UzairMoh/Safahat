using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Models.Enums;

namespace Safahat.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    
    protected BaseController(IAuthorizationService authorizationService = null)
    {
        _authorizationService = authorizationService;
    }
    
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
    /// Checks if the current user is authorized to access a resource
    /// </summary>
    protected async Task<bool> UserCanAccessResourceAsync(Guid resourceOwnerId, string policyName = "ResourceOwnerOrAdmin")
    {
        // Admin can always access
        if (IsAdmin)
            return true;
            
        // For non-admins, they can only access their own resources
        return resourceOwnerId == UserId;
    }
    
    /// <summary>
    /// Standard response formatters
    /// </summary>
    protected ActionResult Success<T>(T data) => Ok(new { success = true, data });
    protected ActionResult NotFoundWithMessage(string message) => NotFound(new { error = message });
    protected ActionResult ForbidWithMessage(string message = "Access denied") => StatusCode(403, new { error = message });
    protected ActionResult BadRequestWithMessage(string message) => BadRequest(new { error = message });
    protected ActionResult CreatedWithMessage<T>(string message, T data, string routePath = null) => 
        Created(routePath ?? Request.Path.Value, new { success = true, message, data });
}