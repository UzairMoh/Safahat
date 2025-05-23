using Microsoft.EntityFrameworkCore;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Implementations;

public class TagRepository(SafahatDbContext context) : Repository<Tag>(context), ITagRepository
{
    public async Task<Tag?> GetBySlugAsync(string slug)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.Slug == slug);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug)
    {
        return !await DbSet.AnyAsync(t => t.Slug == slug);
    }
}