using AutoMapper;
using Safahat.Application.DTOs.Requests.Users;
using Safahat.Application.DTOs.Responses.Users;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;

namespace Safahat.Application.Services;

public class UserService(
    IUserRepository userRepository,
    IPostRepository postRepository,
    ICommentRepository commentRepository,
    IMapper mapper)
    : IUserService
{
    public async Task<IEnumerable<UserListItemResponse>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();
        return mapper.Map<IEnumerable<UserListItemResponse>>(users);
    }

    public async Task<UserDetailResponse> GetUserByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var userDetail = mapper.Map<UserDetailResponse>(user);
        
        var posts = await postRepository.GetPostsByAuthorAsync(id);
        var comments = await commentRepository.GetCommentsByUserAsync(id);
        
        userDetail.PostCount = posts.Count();
        userDetail.CommentCount = comments.Count();
        
        return userDetail;
    }

    public async Task<UserDetailResponse> GetUserByUsernameAsync(string username)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var userDetail = mapper.Map<UserDetailResponse>(user);
        
        var posts = await postRepository.GetPostsByAuthorAsync(user.Id);
        var comments = await commentRepository.GetCommentsByUserAsync(user.Id);
        
        userDetail.PostCount = posts.Count();
        userDetail.CommentCount = comments.Count();
        
        return userDetail;
    }

    public async Task<UserDetailResponse> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);
        return await GetUserByIdAsync(userId);
    }

    public async Task<UserDetailResponse> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);
        return await GetUserByIdAsync(userId);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        user.IsActive = false;
        user.Email = $"deleted_{userId}@example.com";
        user.Username = $"deleted_user_{userId}";
        user.FirstName = "Deleted";
        user.LastName = "User";
        user.Bio = null;
        user.ProfilePictureUrl = null;
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<UserStatisticsResponse> GetUserStatisticsAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var posts = await postRepository.GetPostsByAuthorAsync(userId);
        var comments = await commentRepository.GetCommentsByUserAsync(userId);

        return new UserStatisticsResponse
        {
            TotalPosts = posts.Count(),
            PublishedPosts = posts.Count(p => p.Status == Models.Enums.PostStatus.Published),
            DraftPosts = posts.Count(p => p.Status == Models.Enums.PostStatus.Draft),
            TotalComments = comments.Count(),
            ApprovedComments = comments.Count(c => c.IsApproved),
            PendingComments = comments.Count(c => !c.IsApproved)
        };
    }
}