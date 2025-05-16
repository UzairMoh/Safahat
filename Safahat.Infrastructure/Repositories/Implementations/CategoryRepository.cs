using Microsoft.EntityFrameworkCore;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Implementations;

public class CategoryRepository(SafahatDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public async Task<Category> GetBySlugAsync(string slug)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug)
    {
        return !await _dbSet.AnyAsync(c => c.Slug == slug);
    }
}