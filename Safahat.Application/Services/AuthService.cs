using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Safahat.Application.DTOs.Requests;
using Safahat.Application.DTOs.Responses;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;
using Safahat.Models.Enums;

namespace Safahat.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IMapper mapper,
    IConfiguration configuration)
    : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        
        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new ApplicationException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new ApplicationException("Account is inactive");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(user);
        var expiration = DateTime.UtcNow.AddDays(7); // Token expires in 7 days

        return new AuthResponse
        {
            Token = token,
            User = mapper.Map<UserResponse>(user),
            Expiration = expiration
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if email is already taken
        var existingEmail = await userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new ApplicationException("Email is already registered");
        }

        // Check if username is already taken
        var existingUsername = await userRepository.GetByUsernameAsync(request.Username);
        if (existingUsername != null)
        {
            throw new ApplicationException("Username is already taken");
        }

        // Map request to user entity
        var user = mapper.Map<User>(request);
        
        // Hash password
        user.PasswordHash = HashPassword(request.Password);
        
        // Set default user role to Reader
        user.Role = UserRole.Reader;
        
        // Save user to database
        var createdUser = await userRepository.AddAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(createdUser);
        var expiration = DateTime.UtcNow.AddDays(7); // Token expires in 7 days

        return new AuthResponse
        {
            Token = token,
            User = mapper.Map<UserResponse>(createdUser),
            Expiration = expiration
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        // Verify current password
        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new ApplicationException("Current password is incorrect");
        }

        // Hash new password
        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Update user in database
        await userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<UserResponse> GetUserProfileAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        return mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> UpdateUserProfileAsync(int userId, UpdateUserProfileRequest request)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        // Update user properties
        mapper.Map(request, user);
        user.UpdatedAt = DateTime.UtcNow;

        // Update user in database
        await userRepository.UpdateAsync(user);

        return mapper.Map<UserResponse>(user);
    }

    #region Helper Methods

    private string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the password with the salt
        byte[] hash = GetHash(password, salt);

        // Combine the salt and hash
        byte[] hashWithSalt = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, hashWithSalt, 0, salt.Length);
        Array.Copy(hash, 0, hashWithSalt, salt.Length, hash.Length);

        // Convert to base64 string
        return Convert.ToBase64String(hashWithSalt);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        // Convert the stored hash from base64 string
        byte[] hashWithSalt = Convert.FromBase64String(storedHash);

        // Extract the salt (first 16 bytes)
        byte[] salt = new byte[16];
        Array.Copy(hashWithSalt, 0, salt, 0, salt.Length);

        // Extract the hash (remaining bytes)
        byte[] storedHashBytes = new byte[hashWithSalt.Length - salt.Length];
        Array.Copy(hashWithSalt, salt.Length, storedHashBytes, 0, storedHashBytes.Length);

        // Hash the provided password with the extracted salt
        byte[] computedHash = GetHash(password, salt);

        // Compare the computed hash with the stored hash
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != storedHashBytes[i])
            {
                return false;
            }
        }

        return true;
    }

    private byte[] GetHash(string password, byte[] salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(32); // 256 bits
        }
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("role", user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion
}