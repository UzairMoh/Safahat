using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
            
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);
                
        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(100);
                
        builder.Property(c => c.Description)
            .HasMaxLength(500);
                
        // Create index for slug
        builder.HasIndex(c => c.Slug).IsUnique();
    }
}