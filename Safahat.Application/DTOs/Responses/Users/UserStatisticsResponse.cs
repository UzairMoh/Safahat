namespace Safahat.Application.DTOs.Responses.Users;

public class UserStatisticsResponse
{
    public int TotalPosts { get; set; }
    public int PublishedPosts { get; set; }
    public int DraftPosts { get; set; }
    public int TotalComments { get; set; }
}