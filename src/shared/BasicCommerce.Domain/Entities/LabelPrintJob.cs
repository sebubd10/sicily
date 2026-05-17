using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class LabelPrintJob : TenantEntity
{
    public string JobNumber { get; private set; } = default!;
    public Guid StoreId { get; private set; }
    public Guid TemplateId { get; private set; }
    public LabelOutputFormat OutputFormat { get; private set; }
    public LabelPrintJobStatus JobStatus { get; private set; } = LabelPrintJobStatus.Pending;
    public string? PrinterName { get; private set; }
    public string? Notes { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? PrintedByUserId { get; private set; }
    public DateTime? PrintedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int TotalLabels { get; private set; }

    private readonly List<LabelPrintJobItem> _items = [];
    public IReadOnlyCollection<LabelPrintJobItem> Items => _items.AsReadOnly();

    private LabelPrintJob() { }

    public static LabelPrintJob Create(Guid tenantId, Guid storeId, Guid templateId,
        LabelOutputFormat outputFormat, Guid createdByUserId,
        string? printerName, string? notes)
        => new()
        {
            TenantId = tenantId,
            StoreId = storeId,
            TemplateId = templateId,
            OutputFormat = outputFormat,
            CreatedByUserId = createdByUserId,
            PrinterName = printerName,
            Notes = notes,
            JobNumber = GenerateJobNumber()
        };

    public LabelPrintJobItem AddItem(Guid productId, int quantity,
        decimal? overridePrice = null, string? customText = null,
        string? lotNumber = null, DateTime? expiryDate = null)
    {
        if (JobStatus != LabelPrintJobStatus.Pending)
            throw new DomainException("Cannot add items to a non-pending print job.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId
            && i.LotNumber == lotNumber);
        if (existing is not null)
        {
            existing.AddQuantity(quantity);
            TotalLabels += quantity;
            UpdatedAt = DateTime.UtcNow;
            return existing;
        }

        var item = LabelPrintJobItem.Create(Id, productId, quantity,
            overridePrice, customText, lotNumber, expiryDate);
        _items.Add(item);
        TotalLabels += quantity;
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public void RemoveItem(Guid itemId)
    {
        if (JobStatus != LabelPrintJobStatus.Pending)
            throw new DomainException("Cannot remove items from a non-pending print job.");
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException("Item not found in print job.");
        TotalLabels -= item.Quantity;
        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPrinting(Guid printedBy)
    {
        if (JobStatus != LabelPrintJobStatus.Pending)
            throw new DomainException("Only pending jobs can be started.");
        if (!_items.Any())
            throw new DomainException("Print job has no items.");
        JobStatus = LabelPrintJobStatus.Printing;
        PrintedByUserId = printedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        if (JobStatus != LabelPrintJobStatus.Printing)
            throw new DomainException("Only printing jobs can be completed.");
        JobStatus = LabelPrintJobStatus.Completed;
        PrintedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        if (JobStatus is LabelPrintJobStatus.Completed or LabelPrintJobStatus.Cancelled)
            throw new DomainException("Cannot fail a completed or cancelled job.");
        JobStatus = LabelPrintJobStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (JobStatus is LabelPrintJobStatus.Completed or LabelPrintJobStatus.Failed)
            throw new DomainException("Cannot cancel a completed or failed job.");
        JobStatus = LabelPrintJobStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateJobNumber()
        => $"LBL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
}

public class LabelPrintJobItem : BaseEntity
{
    public Guid PrintJobId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal? OverridePrice { get; private set; }
    public string? CustomText { get; private set; }
    public string? LotNumber { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    private LabelPrintJobItem() { }

    internal static LabelPrintJobItem Create(Guid printJobId, Guid productId, int quantity,
        decimal? overridePrice, string? customText, string? lotNumber, DateTime? expiryDate)
        => new()
        {
            PrintJobId = printJobId,
            ProductId = productId,
            Quantity = quantity,
            OverridePrice = overridePrice,
            CustomText = customText,
            LotNumber = lotNumber,
            ExpiryDate = expiryDate
        };

    internal void AddQuantity(int qty) => Quantity += qty;
}
