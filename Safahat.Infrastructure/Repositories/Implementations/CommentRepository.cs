using Microsoft.EntityFrameworkCore;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Implementations;

public class CommentRepository(SafahatDbContext context) : Repository<Comment>(context), ICommentRepository
{
    public override async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(c => c.User)
            .Include(c => c.Post)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostAsync(Guid postId)
    {
        return await DbSet
            .Where(c => c.PostId == postId)
            .Include(c => c.User)
            .Include(c => c.Post)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetCommentsByUserAsync(Guid userId)
    {
        return await DbSet
            .Where(c => c.UserId == userId)
            .Include(c => c.User)
            .Include(c => c.Post)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}