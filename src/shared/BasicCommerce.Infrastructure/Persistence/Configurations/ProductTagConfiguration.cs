using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.TenantId, t.Name }).IsUnique();
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();

        builder.HasMany(t => t.Products)
            .WithMany(p => p.Tags)
            .UsingEntity<Dictionary<string, object>>("ProductTagAssignments",
                l => l.HasOne<Product>().WithMany()
                    .HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<ProductTag>().WithMany()
                    .HasForeignKey("ProductTagId").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ProductId", "ProductTagId");
                    j.ToTable("ProductTagAssignments");
                });
    }
}
