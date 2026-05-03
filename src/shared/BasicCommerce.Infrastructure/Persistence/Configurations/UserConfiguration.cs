using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => new { u.TenantId, u.EmployeeCode }).IsUnique();
        builder.HasIndex(u => u.GoogleId).HasFilter("\"GoogleId\" IS NOT NULL");
        builder.HasIndex(u => u.MicrosoftId).HasFilter("\"MicrosoftId\" IS NOT NULL");

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.EmployeeCode).HasMaxLength(20).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.GoogleId).HasMaxLength(200);
        builder.Property(u => u.MicrosoftId).HasMaxLength(200);
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);
        builder.Property(u => u.AuthProvider).HasConversion<string>().HasMaxLength(20);
        builder.Property(u => u.PreferredLanguage).HasMaxLength(10).HasDefaultValue("en");
    }
}
