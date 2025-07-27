using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests;
using Safahat.Application.DTOs.Responses;
using Safahat.Application.DTOs.Responses.Auth;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

[Produces("application/json")]
public class AuthController(IAuthService authService) : BaseController
{
    /// <summary>
    /// Authenticates user and returns JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(400)]
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
    /// Registers a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await authService.RegisterAsync(request);
            return Created($"/api/auth/profile", response);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Changes the authenticated user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            await authService.ChangePasswordAsync(UserId, request);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves the authenticated user's profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        try
        {
            var profile = await authService.GetUserProfileAsync(UserId);
            return Ok(profile);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Updates the authenticated user's profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<UserResponse>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var updatedProfile = await authService.UpdateUserProfileAsync(UserId, request);
            return Ok(updatedProfile);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}