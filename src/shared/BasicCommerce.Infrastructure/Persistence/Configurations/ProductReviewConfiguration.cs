using BasicCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.TenantId, r.ProductId, r.CreatedAt });
        builder.HasIndex(r => new { r.TenantId, r.CustomerId });

        builder.Property(r => r.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(300).IsRequired();
        builder.Property(r => r.ReviewText).HasMaxLength(3000).IsRequired();

        builder.HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Customer)
            .WithMany()
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Details)
            .WithOne()
            .HasForeignKey(d => d.ProductReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductReviewDetailConfiguration : IEntityTypeConfiguration<ProductReviewDetail>
{
    public void Configure(EntityTypeBuilder<ProductReviewDetail> builder)
    {
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => new { d.ProductReviewId, d.CreatedAt });

        builder.Property(d => d.Comment).HasMaxLength(2000).IsRequired();
        builder.Property(d => d.CommenterName).HasMaxLength(200);
    }
}

public class ProductReviewHelpfulnessConfiguration
    : IEntityTypeConfiguration<ProductReviewHelpfulness>
{
    public void Configure(EntityTypeBuilder<ProductReviewHelpfulness> builder)
    {
        builder.HasKey(h => h.Id);
        builder.HasIndex(h => new { h.ProductReviewId, h.CustomerId }).IsUnique();
    }
}
