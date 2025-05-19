using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Users;
using Safahat.Application.DTOs.Responses.Users;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

public class UsersController(IUserService userService) : BaseController
{
    /// <summary>
    /// Get all users - Admin only
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<UserListItemResponse>>> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return Success(users);
    }

    /// <summary>
    /// Get user by ID - User can only access their own profile, Admin can access any
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<UserDetailResponse>> GetUserById(int id)
    {
        try
        {
            // Check if user can access this resource
            if (!await UserCanAccessResourceAsync(id))
            {
                return ForbidWithMessage();
            }
            
            var user = await userService.GetUserByIdAsync(id);
            return Success(user);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    [HttpGet("username/{username}")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<UserDetailResponse>> GetUserByUsername(string username)
    {
        try
        {
            var user = await userService.GetUserByUsernameAsync(username);
            
            // Check if user can access this resource
            if (!await UserCanAccessResourceAsync(user.Id))
            {
                return ForbidWithMessage();
            }
            
            return Success(user);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Update user role - Admin only
    /// </summary>
    [HttpPut("{id:int}/role")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDetailResponse>> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var updatedUser = await userService.UpdateUserRoleAsync(id, request);
            return Success(updatedUser);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Update user status - Admin only
    /// </summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserDetailResponse>> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
    {
        try
        {
            var updatedUser = await userService.UpdateUserStatusAsync(id, request);
            return Success(updatedUser);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Delete user - Admin only
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            // Prevent admin from deleting themselves
            if (id == UserId)
            {
                return BadRequestWithMessage("Cannot delete your own account");
            }
            
            var result = await userService.DeleteUserAsync(id);
            return Success(new { deleted = result });
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }

    /// <summary>
    /// Get user statistics - User can only access their own stats, Admin can access any
    /// </summary>
    [HttpGet("{id:int}/statistics")]
    [Authorize(Policy = "AuthenticatedUser")]
    public async Task<ActionResult<UserStatisticsResponse>> GetUserStatistics(int id)
    {
        try
        {
            // Check if user can access this resource
            if (!await UserCanAccessResourceAsync(id))
            {
                return ForbidWithMessage();
            }
            
            var statistics = await userService.GetUserStatisticsAsync(id);
            return Success(statistics);
        }
        catch (ApplicationException ex)
        {
            return NotFoundWithMessage(ex.Message);
        }
    }
}