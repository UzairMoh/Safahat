using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Safahat.Models.Entities;

namespace Safahat.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
            
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(u => u.PasswordHash)
            .IsRequired();
            
        builder.Property(u => u.FirstName)
            .HasMaxLength(50);
            
        builder.Property(u => u.LastName)
            .HasMaxLength(50);
            
        builder.Property(u => u.Bio)
            .HasMaxLength(500);
            
        builder.Property(u => u.ProfilePictureUrl)
            .HasMaxLength(255);
            
        // Create unique index for username and email
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
    }
}