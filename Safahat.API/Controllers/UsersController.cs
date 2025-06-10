using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Safahat.Application.DTOs.Requests.Users;
using Safahat.Application.DTOs.Responses.Users;
using Safahat.Application.Interfaces;

namespace Safahat.Controllers;

[Produces("application/json")]
public class UsersController(IUserService userService) : BaseController
{
    /// <summary>
    /// Retrieves all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IEnumerable<UserListItemResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<IEnumerable<UserListItemResponse>>> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Retrieves a specific user by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(UserDetailResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDetailResponse>> GetUserById(Guid id)
    {
        try
        {
            if (!UserCanAccessResource(id))
            {
                return Forbid();
            }
            
            var user = await userService.GetUserByIdAsync(id);
            return Ok(user);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a specific user by username
    /// </summary>
    [HttpGet("username/{username}")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(UserDetailResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDetailResponse>> GetUserByUsername(string username)
    {
        try
        {
            var user = await userService.GetUserByUsernameAsync(username);
            
            if (!UserCanAccessResource(user.Id))
            {
                return Forbid();
            }
            
            return Ok(user);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Updates a user's role (Admin only)
    /// </summary>
    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UserDetailResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDetailResponse>> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var updatedUser = await userService.UpdateUserRoleAsync(id, request);
            return Ok(updatedUser);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Updates a user's status (Admin only)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(UserDetailResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDetailResponse>> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        try
        {
            var updatedUser = await userService.UpdateUserStatusAsync(id, request);
            return Ok(updatedUser);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a user (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        try
        {
            if (id == UserId)
            {
                return BadRequest("Cannot delete your own account");
            }
            
            await userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves user statistics
    /// </summary>
    [HttpGet("{id:guid}/statistics")]
    [Authorize(Policy = "AuthenticatedUser")]
    [ProducesResponseType(typeof(UserStatisticsResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserStatisticsResponse>> GetUserStatistics(Guid id)
    {
        try
        {
            if (!UserCanAccessResource(id))
            {
                return Forbid();
            }
            
            var statistics = await userService.GetUserStatisticsAsync(id);
            return Ok(statistics);
        }
        catch (ApplicationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}