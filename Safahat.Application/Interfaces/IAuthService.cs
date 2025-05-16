using Safahat.Application.DTOs.Requests;
using Safahat.Application.DTOs.Responses;

namespace Safahat.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<UserResponse> GetUserProfileAsync(int userId);
    Task<UserResponse> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request);
}