using BasicCommerce.Contracts.Labels;
using BasicCommerce.Domain.Entities;

namespace BasicCommerce.Application.Features.Labels;

public static class LabelMapper
{
    public static LabelTemplateResponse ToResponse(LabelTemplate t)
        => new(t.Id, t.Name, t.Description, t.LabelType.ToString(),
            t.WidthMm, t.HeightMm, t.PrinterDpi, t.LayoutJson,
            t.DefaultBarcodeSymbology.ToString(), t.IsDefault, t.SortOrder,
            t.CreatedAt, t.UpdatedAt);

    public static LabelTemplateSummaryResponse ToSummary(LabelTemplate t)
        => new(t.Id, t.Name, t.LabelType.ToString(), t.WidthMm, t.HeightMm,
            t.DefaultBarcodeSymbology.ToString(), t.IsDefault, t.SortOrder);

    public static LabelPrintJobResponse ToResponse(LabelPrintJob job, string templateName,
        IEnumerable<(LabelPrintJobItem Item, Product Product)> itemsWithProducts)
        => new(
            job.Id, job.JobNumber, job.StoreId, job.TemplateId, templateName,
            job.OutputFormat.ToString(), job.JobStatus.ToString(),
            job.PrinterName, job.Notes, job.TotalLabels,
            job.CreatedByUserId, job.PrintedByUserId, job.PrintedAt,
            job.FailureReason, job.CreatedAt,
            itemsWithProducts.Select(x => new LabelPrintJobItemResponse(
                x.Item.Id, x.Item.ProductId,
                x.Product.Name, x.Product.Sku, x.Product.Barcode,
                x.Item.Quantity, x.Item.OverridePrice, x.Item.CustomText,
                x.Item.LotNumber, x.Item.ExpiryDate
            )).ToList().AsReadOnly());

    public static LabelPrintJobSummaryResponse ToSummary(LabelPrintJob job, string templateName)
        => new(job.Id, job.JobNumber, job.StoreId, templateName,
            job.OutputFormat.ToString(), job.JobStatus.ToString(),
            job.TotalLabels, job.CreatedAt, job.PrintedAt);
}
