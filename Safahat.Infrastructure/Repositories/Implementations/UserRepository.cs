using Microsoft.EntityFrameworkCore;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Implementations;

public class UserRepository(SafahatDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Username == username);
    }
        
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email == email);
    }
        
    public async Task<bool> IsUsernameUniqueAsync(string username)
    {
        return !await DbSet.AnyAsync(u => u.Username == username);
    }
        
    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return !await DbSet.AnyAsync(u => u.Email == email);
    }
}