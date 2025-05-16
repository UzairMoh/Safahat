using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetCommentsByPostAsync(int postId);
    Task<IEnumerable<Comment>> GetCommentsByUserAsync(int userId);
    Task<IEnumerable<Comment>> GetPendingCommentsAsync();
}