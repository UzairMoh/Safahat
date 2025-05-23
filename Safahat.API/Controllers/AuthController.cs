using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests;
using Safahat.Application.DTOs.Responses;
using Safahat.Application.DTOs.Responses.Auth;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

public class AuthController(IAuthService authService) : BaseController
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
            return Success(response);
        }
        catch (ApplicationException ex)
        {
            return BadRequestWithMessage(ex.Message);
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
            return CreatedWithMessage("User registered successfully", response);
        }
        catch (ApplicationException ex)
        {
            return BadRequestWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Change a user's password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var result = await authService.ChangePasswordAsync(UserId, request);
            return Success(new { success = result });
        }
        catch (ApplicationException ex)
        {
            return BadRequestWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Get the authenticated user's profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        try
        {
            var profile = await authService.GetUserProfileAsync(UserId);
            return Success(profile);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Update the authenticated user's profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<UserResponse>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var updatedProfile = await authService.UpdateUserProfileAsync(UserId, request);
            return Success(updatedProfile);
        }
        catch (ApplicationException ex)
        {
            return BadRequestWithMessage(ex.Message);
        }
    }
}