using System.Globalization;
using System.Text;
using System.Text.Json;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;

namespace BasicCommerce.Infrastructure.Services;

/// <summary>
/// Renders label templates into ZPL II (for thermal/Zebra printers), HTML (for browser printing),
/// or JSON layout (for client-side rendering with a JS barcode library).
/// </summary>
public class LabelRenderService : ILabelRenderService
{
    // ZPL: 1 mm ≈ 8 dots at 203 DPI, 12 dots at 300 DPI
    private static int MmToDots(decimal mm, int dpi) => (int)Math.Round(mm * dpi / 25.4m);

    public LabelRenderResult RenderSingle(LabelTemplate template,
        LabelRenderContext context, LabelOutputFormat format)
    {
        var layout = ParseLayout(template.LayoutJson);
        return format switch
        {
            LabelOutputFormat.Zpl => BuildZpl([context], [1], template, layout, 1),
            LabelOutputFormat.Html => BuildHtml([context], [1], template, layout),
            LabelOutputFormat.JsonLayout => BuildJsonLayout([context], [1], template, layout),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    public LabelRenderResult RenderJob(LabelTemplate template,
        IEnumerable<(LabelRenderContext Context, int Quantity)> items,
        LabelOutputFormat format)
    {
        var itemList = items.ToList();
        var contexts = itemList.Select(i => i.Context).ToList();
        var quantities = itemList.Select(i => i.Quantity).ToList();
        var totalLabels = quantities.Sum();
        var layout = ParseLayout(template.LayoutJson);

        return format switch
        {
            LabelOutputFormat.Zpl => BuildZpl(contexts, quantities, template, layout, totalLabels),
            LabelOutputFormat.Html => BuildHtml(contexts, quantities, template, layout),
            LabelOutputFormat.JsonLayout => BuildJsonLayout(contexts, quantities, template, layout),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    // ──────────────────────────────────────────────────────────────
    // ZPL II Renderer
    // ──────────────────────────────────────────────────────────────
    private static LabelRenderResult BuildZpl(IList<LabelRenderContext> contexts,
        IList<int> quantities, LabelTemplate template, LabelLayout layout, int totalLabels)
    {
        var sb = new StringBuilder();
        var dpi = template.PrinterDpi;
        var printWidth = MmToDots((decimal)layout.Width, dpi);
        var labelLength = MmToDots((decimal)layout.Height, dpi);

        for (var i = 0; i < contexts.Count; i++)
        {
            var ctx = contexts[i];
            var qty = quantities[i];

            for (var q = 0; q < qty; q++)
            {
                sb.AppendLine("^XA");
                sb.AppendLine($"^PW{printWidth}");
                sb.AppendLine($"^LL{labelLength}");
                sb.AppendLine("^CI28"); // UTF-8 encoding

                foreach (var field in layout.Fields)
                {
                    var x = MmToDots((decimal)field.X, dpi);
                    var y = MmToDots((decimal)field.Y, dpi);

                    switch (field.FieldType)
                    {
                        case LabelFieldType.ProductName:
                            AppendZplText(sb, x, y, field, GetProductName(ctx, false));
                            break;
                        case LabelFieldType.ProductNameBn:
                            AppendZplText(sb, x, y, field, GetProductName(ctx, true));
                            break;
                        case LabelFieldType.Price:
                            AppendZplText(sb, x, y, field, FormatPrice(ctx.Price, field));
                            break;
                        case LabelFieldType.OriginalPrice:
                            if (ctx.OriginalPrice.HasValue)
                                AppendZplText(sb, x, y, field, FormatPrice(ctx.OriginalPrice.Value, field));
                            break;
                        case LabelFieldType.CostPrice:
                            if (ctx.Product.CostPrice is not null)
                                AppendZplText(sb, x, y, field, FormatPrice(ctx.Product.CostPrice.Amount, field));
                            break;
                        case LabelFieldType.Barcode:
                            AppendZplBarcode(sb, x, y, field, ctx.Product.Barcode,
                                field.BarcodeType ?? template.DefaultBarcodeSymbology);
                            break;
                        case LabelFieldType.Sku:
                            AppendZplText(sb, x, y, field, ctx.Product.Sku);
                            break;
                        case LabelFieldType.Plu:
                            AppendZplText(sb, x, y, field, ctx.Product.Plu ?? ctx.Product.Sku);
                            break;
                        case LabelFieldType.StoreName:
                            AppendZplText(sb, x, y, field, ctx.Store?.Name ?? string.Empty);
                            break;
                        case LabelFieldType.ExpiryDate:
                            if (ctx.ExpiryDate.HasValue)
                                AppendZplText(sb, x, y, field,
                                    ctx.ExpiryDate.Value.ToString(field.DateFormat ?? "dd/MM/yyyy"));
                            break;
                        case LabelFieldType.LotNumber:
                            if (!string.IsNullOrWhiteSpace(ctx.LotNumber))
                                AppendZplText(sb, x, y, field, ctx.LotNumber);
                            break;
                        case LabelFieldType.WeightUom:
                            if (ctx.WeightKg.HasValue)
                                AppendZplText(sb, x, y, field,
                                    $"{ctx.WeightKg:F3} kg");
                            break;
                        case LabelFieldType.PromotionalText:
                            AppendZplText(sb, x, y, field,
                                ctx.CustomText ?? field.StaticText ?? string.Empty);
                            break;
                        case LabelFieldType.VatIndicator:
                            AppendZplText(sb, x, y, field, "Inc. VAT");
                            break;
                        case LabelFieldType.CategoryName:
                            AppendZplText(sb, x, y, field,
                                ctx.Product.Category?.Name ?? string.Empty);
                            break;
                        case LabelFieldType.ManufacturerName:
                            AppendZplText(sb, x, y, field,
                                ctx.Product.Manufacturer?.Name ?? string.Empty);
                            break;
                        case LabelFieldType.CustomText:
                            AppendZplText(sb, x, y, field, field.StaticText ?? string.Empty);
                            break;
                        case LabelFieldType.Separator:
                            var lineWidth = MmToDots((decimal)(field.Width ?? layout.Width - field.X * 2), dpi);
                            sb.AppendLine($"^FO{x},{y}^GB{lineWidth},1,1^FS");
                            break;
                    }
                }

                sb.AppendLine("^XZ");
            }
        }

        return new LabelRenderResult(sb.ToString(), LabelOutputFormat.Zpl,
            "application/x-zebra-zpl", totalLabels);
    }

    private static void AppendZplText(StringBuilder sb, int x, int y,
        LabelField field, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var fontSize = (int)Math.Round((field.FontSize ?? 8) * 2.5); // pt → dots approx
        var bold = field.FontWeight == "Bold" ? "B" : "R";
        sb.AppendLine($"^FO{x},{y}^A0{bold},{fontSize},{fontSize}^FD{EscapeZpl(text)}^FS");
    }

    private static void AppendZplBarcode(StringBuilder sb, int x, int y,
        LabelField field, string data, BarcodeSymbology symbology)
    {
        var height = MmToDots((decimal)(field.BarcodeHeight ?? 10), 203);
        var showText = field.ShowBarcodeText != false ? "Y" : "N";

        sb.AppendLine($"^FO{x},{y}");
        sb.Append(symbology switch
        {
            BarcodeSymbology.EAN13 => $"^BEN,{height},{showText},N",
            BarcodeSymbology.EAN8 => $"^B8N,{height},{showText},N",
            BarcodeSymbology.UPCA => $"^BUN,{height},{showText},N",
            BarcodeSymbology.Code128 => $"^BCN,{height},{showText},N,N",
            BarcodeSymbology.Code39 => $"^B3N,N,{height},{showText},N",
            BarcodeSymbology.ITF14 => $"^BIN,{height},{showText},N",
            BarcodeSymbology.QRCode =>
                $"^BQN,2,{Math.Max(2, height / 20)}" + $"\n^FDQA,{data}",
            _ => $"^BCN,{height},{showText},N,N"
        });
        if (symbology != BarcodeSymbology.QRCode)
            sb.Append($"^FD{data}^FS");
        else
            sb.Append("^FS");
        sb.AppendLine();
    }

    private static string EscapeZpl(string s) =>
        s.Replace("^", "\\^").Replace("~", "\\~");

    // ──────────────────────────────────────────────────────────────
    // HTML Renderer (for browser / PDF printing)
    // ──────────────────────────────────────────────────────────────
    private static LabelRenderResult BuildHtml(IList<LabelRenderContext> contexts,
        IList<int> quantities, LabelTemplate template, LabelLayout layout)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>");
        sb.AppendLine("@page { margin: 0; }");
        sb.AppendLine("body { margin: 0; padding: 0; }");
        sb.AppendLine(".label-page { page-break-after: always; }");
        sb.AppendLine(".label { position: relative; overflow: hidden; box-sizing: border-box; }");
        sb.AppendLine(".field { position: absolute; white-space: nowrap; overflow: hidden; }");
        sb.AppendLine(".field-wrap { position: absolute; overflow: hidden; word-break: break-word; }");
        sb.AppendLine(".separator { position: absolute; border-top: 1px solid #000; }");
        sb.AppendLine(".strike { text-decoration: line-through; opacity: 0.6; }");
        sb.AppendLine("</style>");
        // JsBarcode CDN for client-side barcode rendering
        sb.AppendLine("<script src='https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js'></script>");
        sb.AppendLine("</head><body>");

        int barcodeCounter = 0;

        for (var i = 0; i < contexts.Count; i++)
        {
            var ctx = contexts[i];
            var qty = quantities[i];

            for (var q = 0; q < qty; q++)
            {
                sb.AppendLine($"<div class='label-page'>");
                sb.AppendLine($"<div class='label' style='width:{layout.Width}mm;height:{layout.Height}mm;border:0.5px solid #ccc;'>");

                foreach (var field in layout.Fields)
                {
                    var posStyle = $"left:{field.X}mm;top:{field.Y}mm;";
                    if (field.Width.HasValue) posStyle += $"width:{field.Width}mm;";
                    if (field.Height.HasValue) posStyle += $"height:{field.Height}mm;";

                    var fontStyle = $"font-size:{field.FontSize ?? 8}pt;";
                    if (field.FontWeight == "Bold") fontStyle += "font-weight:bold;";

                    switch (field.FieldType)
                    {
                        case LabelFieldType.ProductName:
                        case LabelFieldType.ProductNameBn:
                            var nm = field.FieldType == LabelFieldType.ProductNameBn
                                ? ctx.Product.NameBn : ctx.Product.Name;
                            sb.AppendLine($"<div class='field-wrap' style='{posStyle}{fontStyle}'>{HtmlEncode(nm)}</div>");
                            break;

                        case LabelFieldType.Price:
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(FormatPrice(ctx.Price, field))}</div>");
                            break;

                        case LabelFieldType.OriginalPrice:
                            if (ctx.OriginalPrice.HasValue)
                                sb.AppendLine($"<div class='field strike' style='{posStyle}{fontStyle}'>{HtmlEncode(FormatPrice(ctx.OriginalPrice.Value, field))}</div>");
                            break;

                        case LabelFieldType.CostPrice:
                            if (ctx.Product.CostPrice is not null)
                                sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(FormatPrice(ctx.Product.CostPrice.Amount, field))}</div>");
                            break;

                        case LabelFieldType.Barcode:
                            var bcId = $"bc{barcodeCounter++}";
                            var format = (field.BarcodeType ?? template.DefaultBarcodeSymbology) switch
                            {
                                BarcodeSymbology.EAN13 => "EAN13",
                                BarcodeSymbology.EAN8 => "EAN8",
                                BarcodeSymbology.UPCA => "UPC",
                                BarcodeSymbology.Code128 => "CODE128",
                                BarcodeSymbology.Code39 => "CODE39",
                                BarcodeSymbology.ITF14 => "ITF14",
                                BarcodeSymbology.QRCode => "QR",
                                _ => "CODE128"
                            };
                            var bcH = (int)((field.BarcodeHeight ?? 10) * 3.78); // mm to px approx
                            var bcW = (int)((field.Width ?? layout.Width - field.X * 2) * 3.78);
                            sb.AppendLine($"<svg id='{bcId}' class='field' style='{posStyle}' width='{bcW}' height='{bcH}'></svg>");
                            sb.AppendLine($"<script>JsBarcode('#{bcId}','{ctx.Product.Barcode}',{{format:'{format}',height:{bcH},displayValue:{(field.ShowBarcodeText != false ? "true" : "false")},margin:0}});</script>");
                            break;

                        case LabelFieldType.Sku:
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(ctx.Product.Sku)}</div>");
                            break;
                        case LabelFieldType.Plu:
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(ctx.Product.Plu ?? ctx.Product.Sku)}</div>");
                            break;
                        case LabelFieldType.StoreName:
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(ctx.Store?.Name ?? string.Empty)}</div>");
                            break;
                        case LabelFieldType.ExpiryDate:
                            if (ctx.ExpiryDate.HasValue)
                                sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(ctx.ExpiryDate.Value.ToString(field.DateFormat ?? "dd/MM/yyyy"))}</div>");
                            break;
                        case LabelFieldType.LotNumber:
                            if (!string.IsNullOrWhiteSpace(ctx.LotNumber))
                                sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>Lot: {HtmlEncode(ctx.LotNumber)}</div>");
                            break;
                        case LabelFieldType.WeightUom:
                            if (ctx.WeightKg.HasValue)
                                sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{ctx.WeightKg:F3} kg</div>");
                            break;
                        case LabelFieldType.PromotionalText:
                            var promoTxt = ctx.CustomText ?? field.StaticText ?? string.Empty;
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle};color:red;'>{HtmlEncode(promoTxt)}</div>");
                            break;
                        case LabelFieldType.VatIndicator:
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>Inc. VAT</div>");
                            break;
                        case LabelFieldType.CategoryName:
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(ctx.Product.Category?.Name ?? string.Empty)}</div>");
                            break;
                        case LabelFieldType.ManufacturerName:
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(ctx.Product.Manufacturer?.Name ?? string.Empty)}</div>");
                            break;
                        case LabelFieldType.CustomText:
                            sb.AppendLine($"<div class='field' style='{posStyle}{fontStyle}'>{HtmlEncode(field.StaticText ?? string.Empty)}</div>");
                            break;
                        case LabelFieldType.Separator:
                            var sepW = field.Width ?? layout.Width - field.X * 2;
                            sb.AppendLine($"<div class='separator' style='{posStyle}width:{sepW}mm;'></div>");
                            break;
                    }
                }

                sb.AppendLine("</div></div>");
            }
        }

        sb.AppendLine("</body></html>");

        return new LabelRenderResult(sb.ToString(), LabelOutputFormat.Html,
            "text/html", quantities.Sum());
    }

    private static string HtmlEncode(string s) =>
        System.Net.WebUtility.HtmlEncode(s);

    // ──────────────────────────────────────────────────────────────
    // JSON Layout Renderer (for client-side rendering)
    // ──────────────────────────────────────────────────────────────
    private static LabelRenderResult BuildJsonLayout(IList<LabelRenderContext> contexts,
        IList<int> quantities, LabelTemplate template, LabelLayout layout)
    {
        var labels = new List<object>();

        for (var i = 0; i < contexts.Count; i++)
        {
            var ctx = contexts[i];
            var qty = quantities[i];

            var resolvedFields = layout.Fields.Select(f => ResolveJsonField(f, ctx, template)).ToList();

            for (var q = 0; q < qty; q++)
            {
                labels.Add(new
                {
                    widthMm = layout.Width,
                    heightMm = layout.Height,
                    productId = ctx.Product.Id,
                    productSku = ctx.Product.Sku,
                    fields = resolvedFields
                });
            }
        }

        var json = JsonSerializer.Serialize(new { labels }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return new LabelRenderResult(json, LabelOutputFormat.JsonLayout,
            "application/json", quantities.Sum());
    }

    private static object ResolveJsonField(LabelField field, LabelRenderContext ctx,
        LabelTemplate template)
    {
        var value = field.FieldType switch
        {
            LabelFieldType.ProductName => ctx.Product.Name,
            LabelFieldType.ProductNameBn => ctx.Product.NameBn,
            LabelFieldType.Price => FormatPrice(ctx.Price, field),
            LabelFieldType.OriginalPrice => ctx.OriginalPrice.HasValue
                ? FormatPrice(ctx.OriginalPrice.Value, field) : null,
            LabelFieldType.CostPrice => ctx.Product.CostPrice is not null
                ? FormatPrice(ctx.Product.CostPrice.Amount, field) : null,
            LabelFieldType.Barcode => ctx.Product.Barcode,
            LabelFieldType.Sku => ctx.Product.Sku,
            LabelFieldType.Plu => ctx.Product.Plu ?? ctx.Product.Sku,
            LabelFieldType.StoreName => ctx.Store?.Name,
            LabelFieldType.ExpiryDate => ctx.ExpiryDate?.ToString(field.DateFormat ?? "dd/MM/yyyy"),
            LabelFieldType.LotNumber => ctx.LotNumber,
            LabelFieldType.WeightUom => ctx.WeightKg?.ToString("F3") + " kg",
            LabelFieldType.PromotionalText => ctx.CustomText ?? field.StaticText,
            LabelFieldType.VatIndicator => "Inc. VAT",
            LabelFieldType.CategoryName => ctx.Product.Category?.Name,
            LabelFieldType.ManufacturerName => ctx.Product.Manufacturer?.Name,
            LabelFieldType.CustomText => field.StaticText,
            _ => null
        };

        return new
        {
            fieldType = field.FieldType.ToString(),
            x = field.X,
            y = field.Y,
            width = field.Width,
            height = field.Height,
            value,
            fontSize = field.FontSize,
            fontWeight = field.FontWeight,
            alignment = field.Alignment,
            barcodeType = field.BarcodeType?.ToString()
                ?? (field.FieldType == LabelFieldType.Barcode
                    ? template.DefaultBarcodeSymbology.ToString() : null),
            barcodeHeight = field.BarcodeHeight,
            showBarcodeText = field.ShowBarcodeText,
            staticText = field.StaticText,
            dateFormat = field.DateFormat
        };
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────
    private static string GetProductName(LabelRenderContext ctx, bool bengali)
        => bengali ? ctx.Product.NameBn : ctx.Product.Name;

    private static string FormatPrice(decimal amount, LabelField field)
    {
        var prefix = field.PricePrefix ?? "৳";
        return $"{prefix}{amount:N2}";
    }

    private static LabelLayout ParseLayout(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<LabelLayout>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Empty layout JSON.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid label layout JSON: {ex.Message}", ex);
        }
    }
}

// ──────────────────────────────────────────────────────────────
// Layout model (deserialized from LabelTemplate.LayoutJson)
// ──────────────────────────────────────────────────────────────
public class LabelLayout
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double MarginX { get; set; } = 2;
    public double MarginY { get; set; } = 2;
    public List<LabelField> Fields { get; set; } = [];
}

public class LabelField
{
    public LabelFieldType FieldType { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public double? FontSize { get; set; }
    public string? FontWeight { get; set; }  // "Normal" | "Bold"
    public string? Alignment { get; set; }   // "Left" | "Center" | "Right"
    public int? MaxLines { get; set; }
    public BarcodeSymbology? BarcodeType { get; set; }
    public double? BarcodeHeight { get; set; }
    public bool? ShowBarcodeText { get; set; }
    public string? PricePrefix { get; set; }
    public string? DateFormat { get; set; }
    public string? StaticText { get; set; }
}
