using Microsoft.AspNetCore.Mvc;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserRepository userRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await userRepository.GetAllAsync();
        return Ok(users);
    }
        
    // Simple endpoint to test that API is working
    [HttpGet("test")]
    public ActionResult<string> Test()
    {
        return Ok("Users API is working!");
    }
}