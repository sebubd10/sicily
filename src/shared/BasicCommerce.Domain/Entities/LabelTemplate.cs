using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.Exceptions;

namespace BasicCommerce.Domain.Entities;

public class LabelTemplate : TenantEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public LabelType LabelType { get; private set; }

    // Physical dimensions in millimetres
    public decimal WidthMm { get; private set; }
    public decimal HeightMm { get; private set; }

    // Default printer DPI (203 or 300)
    public int PrinterDpi { get; private set; } = 203;

    // JSON describing all field positions/fonts/barcodes (see LabelLayoutJson schema)
    public string LayoutJson { get; private set; } = default!;

    // Default barcode symbology for this template
    public BarcodeSymbology DefaultBarcodeSymbology { get; private set; } = BarcodeSymbology.EAN13;

    public bool IsDefault { get; private set; }
    public int SortOrder { get; private set; }

    private LabelTemplate() { }

    public static LabelTemplate Create(Guid tenantId, string name, string? description,
        LabelType labelType, decimal widthMm, decimal heightMm,
        string layoutJson, BarcodeSymbology defaultSymbology,
        int printerDpi = 203, bool isDefault = false, int sortOrder = 0)
    {
        ValidateDimensions(widthMm, heightMm);
        if (string.IsNullOrWhiteSpace(layoutJson))
            throw new DomainException("Layout JSON is required.");

        return new LabelTemplate
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description,
            LabelType = labelType,
            WidthMm = widthMm,
            HeightMm = heightMm,
            LayoutJson = layoutJson,
            DefaultBarcodeSymbology = defaultSymbology,
            PrinterDpi = printerDpi,
            IsDefault = isDefault,
            SortOrder = sortOrder
        };
    }

    public void Update(string name, string? description, LabelType labelType,
        decimal widthMm, decimal heightMm, string layoutJson,
        BarcodeSymbology defaultSymbology, int printerDpi, int sortOrder)
    {
        ValidateDimensions(widthMm, heightMm);
        if (string.IsNullOrWhiteSpace(layoutJson))
            throw new DomainException("Layout JSON is required.");

        Name = name.Trim();
        Description = description;
        LabelType = labelType;
        WidthMm = widthMm;
        HeightMm = heightMm;
        LayoutJson = layoutJson;
        DefaultBarcodeSymbology = defaultSymbology;
        PrinterDpi = printerDpi;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateDimensions(decimal w, decimal h)
    {
        if (w <= 0 || w > 300)
            throw new DomainException("Label width must be between 1 and 300 mm.");
        if (h <= 0 || h > 600)
            throw new DomainException("Label height must be between 1 and 600 mm.");
    }
}
