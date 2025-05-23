using Safahat.Application.DTOs.Requests;
using Safahat.Application.DTOs.Responses;
using Safahat.Application.DTOs.Responses.Auth;

namespace Safahat.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<UserResponse> GetUserProfileAsync(Guid userId);
    Task<UserResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);
}