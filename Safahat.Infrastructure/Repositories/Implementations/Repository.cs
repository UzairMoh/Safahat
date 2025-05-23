using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Safahat.Infrastructure.Data.Context;
using Safahat.Infrastructure.Repositories.Interfaces;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Repositories.Implementations;

public class Repository<T>(SafahatDbContext context) : IRepository<T>
    where T : BaseEntity
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }
        
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }
        
    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }
        
    public virtual async Task<T> AddAsync(T entity)
    {
        // Ensure the entity has a Guid if not already set
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }
        
        await DbSet.AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }
        
    public virtual async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }
        
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity != null)
        {
            DbSet.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
        
    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        return await DbSet.AnyAsync(e => e.Id == id);
    }
        
    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}