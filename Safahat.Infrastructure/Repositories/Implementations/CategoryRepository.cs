using Microsoft.EntityFrameworkCore;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Implementations;

public class CategoryRepository(SafahatDbContext context) : Repository<Category>(context), ICategoryRepository
{
    public override async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await DbSet
            .Include(c => c.PostCategories)
            .ToListAsync();
    }

    public override async Task<Category?> GetByIdAsync(Guid id)
    {
        return await DbSet
            .Include(c => c.PostCategories)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await DbSet
            .Include(c => c.PostCategories)
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug)
    {
        return !await DbSet.AnyAsync(c => c.Slug == slug);
    }
}