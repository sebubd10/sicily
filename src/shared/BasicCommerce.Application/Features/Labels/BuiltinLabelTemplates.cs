namespace BasicCommerce.Application.Features.Labels;

/// <summary>
/// Provides ready-to-use JSON layouts that can be used to seed label templates.
/// Width/height are in millimetres; x/y/fontSize are also millimetres/points.
/// </summary>
public static class BuiltinLabelTemplates
{
    // ── 40×25 mm Standard Price Label ─────────────────────────
    public const string PriceLabel40x25Json = """
        {
          "width": 40, "height": 25, "marginX": 1.5, "marginY": 1.5,
          "fields": [
            { "fieldType": "ProductName", "x": 1.5, "y": 1.5,  "width": 37, "fontSize": 6,  "fontWeight": "Bold",   "maxLines": 2 },
            { "fieldType": "Price",       "x": 1.5, "y": 9,    "width": 20, "fontSize": 12, "fontWeight": "Bold",   "pricePrefix": "৳" },
            { "fieldType": "VatIndicator","x": 23,  "y": 11,   "fontSize": 5  },
            { "fieldType": "Separator",   "x": 1.5, "y": 17.5, "width": 37  },
            { "fieldType": "Barcode",     "x": 1.5, "y": 18.5, "width": 37, "barcodeHeight": 5.5, "showBarcodeText": true }
          ]
        }
        """;

    // ── 58×30 mm Shelf Label ───────────────────────────────────
    public const string ShelfLabel58x30Json = """
        {
          "width": 58, "height": 30, "marginX": 2, "marginY": 2,
          "fields": [
            { "fieldType": "ProductName", "x": 2, "y": 2,  "width": 54, "fontSize": 8,  "fontWeight": "Bold", "maxLines": 2 },
            { "fieldType": "Price",       "x": 2, "y": 11, "width": 30, "fontSize": 14, "fontWeight": "Bold", "pricePrefix": "৳" },
            { "fieldType": "Sku",         "x": 2, "y": 22, "fontSize": 6 },
            { "fieldType": "Barcode",     "x": 2, "y": 18, "width": 54, "barcodeHeight": 8, "showBarcodeText": true }
          ]
        }
        """;

    // ── 38×22 mm Small Barcode Label ──────────────────────────
    public const string BarcodeLabelSmallJson = """
        {
          "width": 38, "height": 22, "marginX": 1, "marginY": 1,
          "fields": [
            { "fieldType": "ProductName", "x": 1, "y": 1,   "width": 36, "fontSize": 5.5, "fontWeight": "Bold", "maxLines": 1 },
            { "fieldType": "Sku",         "x": 1, "y": 6.5, "fontSize": 5 },
            { "fieldType": "Barcode",     "x": 1, "y": 9,   "width": 36, "barcodeHeight": 6.5, "showBarcodeText": true }
          ]
        }
        """;

    // ── 60×40 mm Promotional Label ────────────────────────────
    public const string PromotionalLabel60x40Json = """
        {
          "width": 60, "height": 40, "marginX": 2, "marginY": 2,
          "fields": [
            { "fieldType": "PromotionalText","x": 2,  "y": 2,  "width": 56, "fontSize": 9,  "fontWeight": "Bold", "staticText": "SPECIAL OFFER" },
            { "fieldType": "ProductName",    "x": 2,  "y": 8,  "width": 56, "fontSize": 7,  "fontWeight": "Normal","maxLines": 2 },
            { "fieldType": "OriginalPrice",  "x": 2,  "y": 17, "width": 25, "fontSize": 9,  "fontWeight": "Normal", "pricePrefix": "৳" },
            { "fieldType": "Price",          "x": 30, "y": 15, "width": 28, "fontSize": 14, "fontWeight": "Bold",   "pricePrefix": "৳" },
            { "fieldType": "Separator",      "x": 2,  "y": 28, "width": 56 },
            { "fieldType": "Barcode",        "x": 2,  "y": 29, "width": 56, "barcodeHeight": 8, "showBarcodeText": true }
          ]
        }
        """;

    // ── 58×40 mm Weight / Fresh Deli Label ────────────────────
    public const string WeightPriceLabel58x40Json = """
        {
          "width": 58, "height": 40, "marginX": 2, "marginY": 2,
          "fields": [
            { "fieldType": "StoreName",   "x": 2, "y": 2,    "fontSize": 6, "fontWeight": "Bold" },
            { "fieldType": "ProductName", "x": 2, "y": 6.5,  "width": 54, "fontSize": 8, "fontWeight": "Bold", "maxLines": 2 },
            { "fieldType": "WeightUom",   "x": 2, "y": 16,   "fontSize": 8 },
            { "fieldType": "Price",       "x": 2, "y": 22,   "fontSize": 14, "fontWeight": "Bold", "pricePrefix": "৳" },
            { "fieldType": "ExpiryDate",  "x": 2, "y": 32,   "fontSize": 6, "dateFormat": "dd/MM/yyyy" },
            { "fieldType": "Barcode",     "x": 2, "y": 35,   "width": 54, "barcodeHeight": 6, "showBarcodeText": false, "barcodeType": "Code128" }
          ]
        }
        """;

    // ── 100×50 mm Receiving / Warehouse Label ─────────────────
    public const string ReceivingLabel100x50Json = """
        {
          "width": 100, "height": 50, "marginX": 3, "marginY": 3,
          "fields": [
            { "fieldType": "StoreName",   "x": 3,  "y": 3,  "fontSize": 7, "fontWeight": "Bold" },
            { "fieldType": "ProductName", "x": 3,  "y": 8,  "width": 94, "fontSize": 10, "fontWeight": "Bold", "maxLines": 2 },
            { "fieldType": "Sku",         "x": 3,  "y": 19, "fontSize": 7 },
            { "fieldType": "LotNumber",   "x": 50, "y": 19, "fontSize": 7 },
            { "fieldType": "ExpiryDate",  "x": 3,  "y": 25, "fontSize": 7, "dateFormat": "dd/MM/yyyy" },
            { "fieldType": "Separator",   "x": 3,  "y": 31, "width": 94 },
            { "fieldType": "Barcode",     "x": 3,  "y": 32, "width": 94, "barcodeHeight": 12, "showBarcodeText": true, "barcodeType": "Code128" }
          ]
        }
        """;

    public static IReadOnlyList<BuiltinTemplate> All =>
    [
        new("price-40x25",       "Price Label 40×25mm",         "PriceLabel",       40,  25,  PriceLabel40x25Json),
        new("shelf-58x30",       "Shelf Label 58×30mm",          "ShelfLabel",       58,  30,  ShelfLabel58x30Json),
        new("barcode-38x22",     "Barcode Label 38×22mm",        "BarcodeLabel",     38,  22,  BarcodeLabelSmallJson),
        new("promo-60x40",       "Promotional Label 60×40mm",    "PromotionalLabel", 60,  40,  PromotionalLabel60x40Json),
        new("weight-58x40",      "Weight/Fresh Label 58×40mm",   "WeightPriceLabel", 58,  40,  WeightPriceLabel58x40Json),
        new("receiving-100x50",  "Receiving Label 100×50mm",     "ReceivingLabel",   100, 50,  ReceivingLabel100x50Json),
    ];

    public record BuiltinTemplate(
        string Id,
        string Name,
        string LabelType,
        decimal WidthMm,
        decimal HeightMm,
        string LayoutJson);
}
