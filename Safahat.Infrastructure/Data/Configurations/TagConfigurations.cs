using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
            
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(50);
                
        builder.Property(t => t.Slug)
            .IsRequired()
            .HasMaxLength(50);
                
        // Create index for slug
        builder.HasIndex(t => t.Slug).IsUnique();
    }
}