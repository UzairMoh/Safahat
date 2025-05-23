using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Interfaces;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetBySlugAsync(string slug);
    Task<bool> IsSlugUniqueAsync(string slug);
}