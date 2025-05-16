namespace Safahat.Application.DTOs.Requests;

public class UpdateUserProfileRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Bio { get; set; }
    public string ProfilePictureUrl { get; set; }
}