namespace BasicCommerce.Contracts.Labels;

// ── Templates ──────────────────────────────────────────────────
public record LabelTemplateResponse(
    Guid Id,
    string Name,
    string? Description,
    string LabelType,
    decimal WidthMm,
    decimal HeightMm,
    int PrinterDpi,
    string LayoutJson,
    string DefaultBarcodeSymbology,
    bool IsDefault,
    int SortOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record LabelTemplateSummaryResponse(
    Guid Id,
    string Name,
    string LabelType,
    decimal WidthMm,
    decimal HeightMm,
    string DefaultBarcodeSymbology,
    bool IsDefault,
    int SortOrder);

public record LabelTemplateListResponse(
    IReadOnlyList<LabelTemplateSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record CreateLabelTemplateRequest(
    string Name,
    string? Description,
    string LabelType,
    decimal WidthMm,
    decimal HeightMm,
    string LayoutJson,
    string DefaultBarcodeSymbology = "EAN13",
    int PrinterDpi = 203,
    bool IsDefault = false,
    int SortOrder = 0);

public record UpdateLabelTemplateRequest(
    string Name,
    string? Description,
    string LabelType,
    decimal WidthMm,
    decimal HeightMm,
    string LayoutJson,
    string DefaultBarcodeSymbology = "EAN13",
    int PrinterDpi = 203,
    int SortOrder = 0);

// ── Print Jobs ─────────────────────────────────────────────────
public record LabelPrintJobItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    string ProductBarcode,
    int Quantity,
    decimal? OverridePrice,
    string? CustomText,
    string? LotNumber,
    DateTime? ExpiryDate);

public record LabelPrintJobResponse(
    Guid Id,
    string JobNumber,
    Guid StoreId,
    Guid TemplateId,
    string TemplateName,
    string OutputFormat,
    string JobStatus,
    string? PrinterName,
    string? Notes,
    int TotalLabels,
    Guid CreatedByUserId,
    Guid? PrintedByUserId,
    DateTime? PrintedAt,
    string? FailureReason,
    DateTime CreatedAt,
    IReadOnlyList<LabelPrintJobItemResponse> Items);

public record LabelPrintJobSummaryResponse(
    Guid Id,
    string JobNumber,
    Guid StoreId,
    string TemplateName,
    string OutputFormat,
    string JobStatus,
    int TotalLabels,
    DateTime CreatedAt,
    DateTime? PrintedAt);

public record LabelPrintJobListResponse(
    IReadOnlyList<LabelPrintJobSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record CreatePrintJobRequest(
    Guid StoreId,
    Guid TemplateId,
    string OutputFormat,
    string? PrinterName,
    string? Notes);

public record AddPrintJobItemRequest(
    Guid ProductId,
    int Quantity,
    decimal? OverridePrice = null,
    string? CustomText = null,
    string? LotNumber = null,
    DateTime? ExpiryDate = null);

// ── Rendering ──────────────────────────────────────────────────
public record RenderLabelRequest(
    Guid ProductId,
    string OutputFormat,
    decimal? OverridePrice = null,
    string? CustomText = null,
    string? LotNumber = null,
    DateTime? ExpiryDate = null,
    decimal? WeightKg = null,
    int Copies = 1);

public record RenderLabelResponse(
    string Content,
    string Format,
    string MimeType,
    int LabelCount);

// ── Built-in template layouts (returned from GET /labels/templates/builtins) ──
public record BuiltinTemplateInfo(
    string Id,
    string Name,
    string LabelType,
    decimal WidthMm,
    decimal HeightMm,
    string Description,
    string LayoutJsonPreview);
