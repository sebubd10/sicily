using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class UserTypeConfiguration : IEntityTypeConfiguration<UserType>
{
    public void Configure(EntityTypeBuilder<UserType> b)
    {
        b.ToTable("UserTypes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Color).HasMaxLength(20);
        b.Property(x => x.IsSystem).HasDefaultValue(false);
        b.Property(x => x.SortOrder).HasDefaultValue(0);

        b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();

        b.HasMany(x => x.MenuAccess)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("UserTypeSubMenuAccess",
                j => j.HasOne<AppSubMenu>().WithMany().HasForeignKey("SubMenuId")
                       .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<UserType>().WithMany().HasForeignKey("UserTypeId")
                       .OnDelete(DeleteBehavior.Cascade));

        b.HasMany(x => x.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>("UserTypeApiPermission",
                j => j.HasOne<ApiPermission>().WithMany().HasForeignKey("ApiPermissionId")
                       .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<UserType>().WithMany().HasForeignKey("UserTypeId")
                       .OnDelete(DeleteBehavior.Cascade));
    }
}

public class AppMenuConfiguration : IEntityTypeConfiguration<AppMenu>
{
    public void Configure(EntityTypeBuilder<AppMenu> b)
    {
        b.ToTable("AppMenus");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Icon).HasMaxLength(50);
        b.Property(x => x.SortOrder).HasDefaultValue(0);

        b.HasMany(x => x.SubMenus)
            .WithOne()
            .HasForeignKey(x => x.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AppSubMenuConfiguration : IEntityTypeConfiguration<AppSubMenu>
{
    public void Configure(EntityTypeBuilder<AppSubMenu> b)
    {
        b.ToTable("AppSubMenus");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Icon).HasMaxLength(50);
        b.Property(x => x.Route).HasMaxLength(200).IsRequired();
        b.Property(x => x.PermissionCode).HasMaxLength(100);
        b.Property(x => x.SortOrder).HasDefaultValue(0);

        b.HasIndex(x => x.MenuId);
    }
}

public class ApiPermissionConfiguration : IEntityTypeConfiguration<ApiPermission>
{
    public void Configure(EntityTypeBuilder<ApiPermission> b)
    {
        b.ToTable("ApiPermissions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(100).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Group).HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.Group);
    }
}
