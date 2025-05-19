using Safahat.Application.DTOs.Requests.Users;
using Safahat.Application.DTOs.Responses.Users;

namespace Safahat.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserListItemResponse>> GetAllUsersAsync();
    Task<UserDetailResponse> GetUserByIdAsync(int id);
    Task<UserDetailResponse> GetUserByUsernameAsync(string username);
    Task<UserDetailResponse> UpdateUserRoleAsync(int userId, UpdateUserRoleRequest request);
    Task<UserDetailResponse> UpdateUserStatusAsync(int userId, UpdateUserStatusRequest request);
    Task<bool> DeleteUserAsync(int userId);
    Task<UserStatisticsResponse> GetUserStatisticsAsync(int userId);
}