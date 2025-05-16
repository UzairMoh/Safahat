using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);
            
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);
                
        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Content)
            .IsRequired();
                
        builder.Property(p => p.Summary)
            .HasMaxLength(500);
                
        builder.Property(p => p.FeaturedImageUrl)
            .HasMaxLength(255);
            
        // Create indexes for performance
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsFeatured);
            
        // Define relationships
        builder.HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
    }
}