using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Data.Context;

public class SafahatDbContext(DbContextOptions<SafahatDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
            
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
        
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
        
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
        
    private void UpdateTimestamps()
    {
        var now = DateTime.UtcNow;
            
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}