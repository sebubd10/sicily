using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BasicCommerce.Infrastructure.Services.Reports;

public sealed class ProductReportService : IProductReportService
{
    private const string Accent       = "#1B5E20";
    private const string AccentLight  = "#E8F5E9";
    private const string AccentBorder = "#A5D6A7";
    private const string FilterBg    = "#FFF8E1";
    private const string FilterBorder = "#FFE082";

    static ProductReportService()
        => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Generate(
        IEnumerable<ProductReportRow> products,
        string? search = null,
        bool includeInactive = false,
        string? categoryFilter = null)
    {
        var rows     = products.ToList();
        var total    = rows.Count;
        var active   = rows.Count(r => r.Status == "Active");
        var inactive = total - active;
        var hasFilters = !string.IsNullOrWhiteSpace(search) || includeInactive || categoryFilter is not null;

        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(1.6f, Unit.Centimetre);
            page.DefaultTextStyle(t => t.FontFamily(Fonts.Arial).FontSize(8.5f));

            // ── Header ──────────────────────────────────────────────────────
            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Product Catalogue Report")
                            .FontSize(20).Bold().FontColor(Accent);
                        c.Item().Text("BasicCommerce POS")
                            .FontSize(9.5f).FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(220).AlignRight().Column(c =>
                    {
                        c.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy  HH:mm}")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                        if (total == 0)
                            c.Item().PaddingTop(2)
                                .Text("No products found")
                                .FontSize(8).FontColor(Colors.Red.Darken2);
                    });
                });

                if (hasFilters)
                {
                    col.Item().PaddingTop(5)
                        .Background(FilterBg)
                        .Border(0.5f).BorderColor(FilterBorder)
                        .Padding(5)
                        .Row(row =>
                        {
                            row.AutoItem().Text("Filters: ")
                                .FontSize(8).SemiBold().FontColor(Colors.Orange.Darken3);
                            if (!string.IsNullOrWhiteSpace(search))
                                row.AutoItem().PaddingLeft(4)
                                    .Text($"Search = \"{search}\"")
                                    .FontSize(8).FontColor(Colors.Orange.Darken3);
                            if (categoryFilter is not null)
                                row.AutoItem().PaddingLeft(8)
                                    .Text($"Category = \"{categoryFilter}\"")
                                    .FontSize(8).FontColor(Colors.Orange.Darken3);
                            if (includeInactive)
                                row.AutoItem().PaddingLeft(8)
                                    .Text("Includes inactive")
                                    .FontSize(8).FontColor(Colors.Orange.Darken3);
                        });
                }

                col.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Accent);
            });

            // ── Content ──────────────────────────────────────────────────────
            page.Content().PaddingVertical(10).Column(col =>
            {
                // Summary cards
                col.Item()
                    .Background(AccentLight)
                    .Border(1).BorderColor(AccentBorder)
                    .Padding(9)
                    .Row(row =>
                    {
                        SummaryCard(row, "Total",    total,    Colors.Grey.Darken3);
                        SummaryCard(row, "Active",   active,   Colors.Green.Darken2);
                        SummaryCard(row, "Inactive", inactive, Colors.Red.Darken2);
                    });

                if (total == 0)
                {
                    col.Item().PaddingTop(40).AlignCenter()
                        .Text("No products match the applied filters.")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                    return;
                }

                col.Item().PaddingTop(12).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(20);    // #
                        c.ConstantColumn(58);    // SKU
                        c.ConstantColumn(72);    // Barcode
                        c.RelativeColumn(2.4f);  // Name
                        c.RelativeColumn(1.4f);  // Category
                        c.ConstantColumn(52);    // Price
                        c.ConstantColumn(44);    // Cost
                        c.ConstantColumn(36);    // VAT%
                        c.ConstantColumn(42);    // Unit
                        c.RelativeColumn(1.3f);  // Manufacturer
                        c.ConstantColumn(48);    // Status
                    });

                    table.Header(h =>
                    {
                        void HCell(string text, bool right = false) =>
                            h.Cell().Background(Accent).Padding(4).AlignMiddle()
                             .Element(e => right ? e.AlignRight() : e.AlignLeft())
                             .Text(text).FontSize(8).Bold().FontColor(Colors.White);

                        HCell("#");
                        HCell("SKU");
                        HCell("Barcode");
                        HCell("Name");
                        HCell("Category");
                        HCell("Price", right: true);
                        HCell("Cost", right: true);
                        HCell("VAT%");
                        HCell("Unit");
                        HCell("Manufacturer");
                        HCell("Status");
                    });

                    for (var i = 0; i < rows.Count; i++)
                    {
                        var r  = rows[i];
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        void DCell(string? val, bool center = false, bool mono = false, bool right = false)
                        {
                            var cell = table.Cell().Background(bg).Padding(3.5f);
                            IContainer aligned = center
                                ? cell.AlignCenter().AlignMiddle()
                                : right
                                    ? cell.AlignRight().AlignMiddle()
                                    : cell.AlignMiddle();
                            var txt = aligned.Text(val ?? "—").FontSize(8.5f);
                            if (mono) txt.FontFamily(Fonts.Courier);
                        }

                        DCell((i + 1).ToString(), center: true);
                        table.Cell().Background(bg).Padding(3.5f).AlignMiddle()
                            .Text(r.Sku).FontSize(8).FontFamily(Fonts.Courier);
                        DCell(r.Barcode, mono: true);
                        table.Cell().Background(bg).Padding(3.5f).AlignMiddle()
                            .Text(r.Name).FontSize(8.5f).SemiBold();
                        DCell(r.Category);
                        DCell($"{r.Currency} {r.Price:N2}", right: true);
                        DCell(r.CostPrice.HasValue ? $"{r.Currency} {r.CostPrice:N2}" : "—", right: true);
                        DCell(r.VatRate > 0 ? $"{r.VatRate:G}%" : "0%", center: true);
                        DCell(r.UnitType, center: true);
                        DCell(r.Manufacturer);

                        var statusColor = r.Status == "Active"
                            ? Colors.Green.Darken2
                            : Colors.Red.Darken2;

                        table.Cell().Background(bg).Padding(3.5f).AlignCenter().AlignMiddle()
                            .Text(r.Status).FontSize(8).Bold().FontColor(statusColor);
                    }
                });
            });

            // ── Footer ───────────────────────────────────────────────────────
            page.Footer()
                .BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2)
                .PaddingTop(5)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("BasicCommerce POS  |  Product Catalogue Report")
                        .FontSize(7.5f).FontColor(Colors.Grey.Darken1);

                    row.ConstantItem(80).AlignRight().Text(x =>
                    {
                        x.Span("Page ").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        x.CurrentPageNumber().FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        x.Span(" of ").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        x.TotalPages().FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                    });
                });

        })).GeneratePdf();
    }

    private static void SummaryCard(RowDescriptor row, string label, int value, string color)
    {
        row.RelativeItem().AlignCenter().Column(col =>
        {
            col.Item().Text(value.ToString())
                .FontSize(17).Bold().FontColor(color).AlignCenter();
            col.Item().PaddingTop(2).Text(label)
                .FontSize(8).FontColor(Colors.Grey.Darken2).AlignCenter();
        });
    }
}
