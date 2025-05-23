using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetCommentsByPostAsync(Guid postId);
    Task<IEnumerable<Comment>> GetCommentsByUserAsync(Guid userId);
    Task<IEnumerable<Comment>> GetPendingCommentsAsync();
}