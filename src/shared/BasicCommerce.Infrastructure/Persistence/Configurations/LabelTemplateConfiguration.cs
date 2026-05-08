using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BasicCommerce.Infrastructure.Persistence.Configurations;

public class LabelTemplateConfiguration : IEntityTypeConfiguration<LabelTemplate>
{
    public void Configure(EntityTypeBuilder<LabelTemplate> b)
    {
        b.ToTable("LabelTemplates");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.WidthMm).HasPrecision(6, 2);
        b.Property(x => x.HeightMm).HasPrecision(6, 2);
        b.Property(x => x.PrinterDpi).HasDefaultValue(203);
        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.Property(x => x.IsDefault).HasDefaultValue(false);
        b.Property(x => x.LayoutJson).HasColumnType("nvarchar(max)").IsRequired();

        b.Property(x => x.LabelType)
            .HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.DefaultBarcodeSymbology)
            .HasConversion<string>().HasMaxLength(20);

        b.HasIndex(x => new { x.TenantId, x.LabelType });
        b.HasIndex(x => new { x.TenantId, x.Name });
    }
}

public class LabelPrintJobConfiguration : IEntityTypeConfiguration<LabelPrintJob>
{
    public void Configure(EntityTypeBuilder<LabelPrintJob> b)
    {
        b.ToTable("LabelPrintJobs");
        b.HasKey(x => x.Id);

        b.Property(x => x.JobNumber).HasMaxLength(30).IsRequired();
        b.Property(x => x.PrinterName).HasMaxLength(200);
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.FailureReason).HasMaxLength(1000);
        b.Property(x => x.TotalLabels).HasDefaultValue(0);
        b.Property(x => x.JobStatus)
            .HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(LabelPrintJobStatus.Pending);
        b.Property(x => x.OutputFormat)
            .HasConversion<string>().HasMaxLength(20);

        b.HasIndex(x => x.JobNumber).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.StoreId, x.JobStatus });

        b.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.PrintJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LabelPrintJobItemConfiguration : IEntityTypeConfiguration<LabelPrintJobItem>
{
    public void Configure(EntityTypeBuilder<LabelPrintJobItem> b)
    {
        b.ToTable("LabelPrintJobItems");
        b.HasKey(x => x.Id);

        b.Property(x => x.OverridePrice).HasPrecision(18, 4);
        b.Property(x => x.CustomText).HasMaxLength(200);
        b.Property(x => x.LotNumber).HasMaxLength(100);

        b.HasIndex(x => x.PrintJobId);
    }
}
