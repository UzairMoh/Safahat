using AutoMapper;
using Safahat.Application.DTOs.Requests.Users;
using Safahat.Application.DTOs.Responses.Users;
using Safahat.Application.Interfaces;
using Safahat.Infrastructure.Repositories.Interfaces;

namespace Safahat.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserListItemResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserListItemResponse>>(users);
    }

    public async Task<UserDetailResponse> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var userDetail = _mapper.Map<UserDetailResponse>(user);
        
        // Get post and comment counts
        var posts = await _postRepository.GetPostsByAuthorAsync(id);
        var comments = await _commentRepository.GetCommentsByUserAsync(id);
        
        userDetail.PostCount = posts.Count();
        userDetail.CommentCount = comments.Count();
        
        return userDetail;
    }

    public async Task<UserDetailResponse> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var userDetail = _mapper.Map<UserDetailResponse>(user);
        
        // Get post and comment counts
        var posts = await _postRepository.GetPostsByAuthorAsync(user.Id);
        var comments = await _commentRepository.GetCommentsByUserAsync(user.Id);
        
        userDetail.PostCount = posts.Count();
        userDetail.CommentCount = comments.Count();
        
        return userDetail;
    }

    public async Task<UserDetailResponse> UpdateUserRoleAsync(int userId, UpdateUserRoleRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return await GetUserByIdAsync(userId);
    }

    public async Task<UserDetailResponse> UpdateUserStatusAsync(int userId, UpdateUserStatusRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return await GetUserByIdAsync(userId);
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        // Instead of actually deleting, you might want to soft delete
        // by setting IsActive to false and anonymizing data
        user.IsActive = false;
        user.Email = $"deleted_{userId}@example.com";
        user.Username = $"deleted_user_{userId}";
        user.FirstName = "Deleted";
        user.LastName = "User";
        user.Bio = null;
        user.ProfilePictureUrl = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return true;
        
        // Or if you want to actually delete the user:
        // await _userRepository.DeleteAsync(userId);
        // return true;
    }

    public async Task<UserStatisticsResponse> GetUserStatisticsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException("User not found");
        }

        var posts = await _postRepository.GetPostsByAuthorAsync(userId);
        var comments = await _commentRepository.GetCommentsByUserAsync(userId);

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