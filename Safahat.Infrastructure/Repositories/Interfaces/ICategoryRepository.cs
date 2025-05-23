using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
    Task<bool> IsSlugUniqueAsync(string slug);
}