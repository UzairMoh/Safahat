using Microsoft.EntityFrameworkCore;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Implementations;

public class CommentRepository(SafahatDbContext context) : Repository<Comment>(context), ICommentRepository
{
    public async Task<IEnumerable<Comment>> GetCommentsByPostAsync(Guid postId)
    {
        return await DbSet
            .Where(c => c.PostId == postId && c.IsApproved)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetCommentsByUserAsync(Guid userId)
    {
        return await DbSet
            .Where(c => c.UserId == userId)
            .Include(c => c.Post)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetPendingCommentsAsync()
    {
        return await DbSet
            .Where(c => !c.IsApproved)
            .Include(c => c.Post)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}