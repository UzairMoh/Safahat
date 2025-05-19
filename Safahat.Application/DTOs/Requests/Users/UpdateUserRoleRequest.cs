using Safahat.Models.Enums;

namespace Safahat.Application.DTOs.Requests.Users;

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}