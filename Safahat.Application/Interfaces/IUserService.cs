using Safahat.Application.DTOs.Requests.Users;
using Safahat.Application.DTOs.Responses.Users;

namespace Safahat.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserListItemResponse>> GetAllUsersAsync();
    Task<UserDetailResponse> GetUserByIdAsync(Guid id);
    Task<UserDetailResponse> GetUserByUsernameAsync(string username);
    Task<UserDetailResponse> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request);
    Task<UserDetailResponse> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<UserStatisticsResponse> GetUserStatisticsAsync(Guid userId);
}