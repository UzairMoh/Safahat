using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests;
using Safahat.Application.DTOs.Responses;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Login a user and return a JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await authService.LoginAsync(request);
            return Ok(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Change a user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Get the authenticated user's ID from claims
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await authService.ChangePasswordAsync(userId, request);
            return Ok(new { success = result });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get the authenticated user's profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        try
        {
            // Get the authenticated user's ID from claims
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var profile = await authService.GetUserProfileAsync(userId);
            return Ok(profile);
        }
        catch (ApplicationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update the authenticated user's profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            // Get the authenticated user's ID from claims
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var updatedProfile = await authService.UpdateUserProfileAsync(userId, request);
            return Ok(updatedProfile);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}