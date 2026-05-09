using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Products;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BasicCommerce.Infrastructure.Services.Reports;

public sealed class CategoryReportService : ICategoryReportService
{
    private const string Accent      = "#1565C0";
    private const string AccentLight = "#E3F2FD";
    private const string AccentBorder = "#BBDEFB";
    private const string FilterBg   = "#FFF8E1";
    private const string FilterBorder = "#FFE082";

    static CategoryReportService()
        => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Generate(
        IEnumerable<CategoryResponse> categories,
        string? search = null,
        bool includeInactive = false)
    {
        var rows = categories
            .OrderBy(c => c.ParentCategoryId.HasValue)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToList();

        var total    = rows.Count;
        var active   = rows.Count(r => r.Status == "Active");
        var inactive = total - active;
        var root     = rows.Count(r => !r.ParentCategoryId.HasValue);
        var sub      = total - root;

        var hasFilters = !string.IsNullOrWhiteSpace(search) || includeInactive;

        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(t => t.FontFamily(Fonts.Arial).FontSize(9));

            // ── Header ─────────────────────────────────────────────────────────
            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Category Report")
                            .FontSize(22).Bold().FontColor(Accent);
                        c.Item().Text("BasicCommerce POS")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(190).AlignRight().Column(c =>
                    {
                        c.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy  HH:mm}")
                            .FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                        if (rows.Count == 0)
                            c.Item().PaddingTop(2)
                                .Text("No categories found")
                                .FontSize(8).FontColor(Colors.Red.Darken2);
                    });
                });

                // Active filters bar
                if (hasFilters)
                {
                    col.Item().PaddingTop(6)
                        .Background(FilterBg)
                        .Border(0.5f).BorderColor(FilterBorder)
                        .Padding(6)
                        .Row(row =>
                        {
                            row.AutoItem().Text("Filters applied: ")
                                .FontSize(8).SemiBold().FontColor(Colors.Orange.Darken3);

                            if (!string.IsNullOrWhiteSpace(search))
                                row.AutoItem().PaddingLeft(4)
                                    .Text($"Search = \"{search}\"")
                                    .FontSize(8).FontColor(Colors.Orange.Darken3);

                            if (includeInactive)
                                row.AutoItem().PaddingLeft(8)
                                    .Text("Includes inactive")
                                    .FontSize(8).FontColor(Colors.Orange.Darken3);
                        });
                }

                col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Accent);
            });

            // ── Content ────────────────────────────────────────────────────────
            page.Content().PaddingVertical(12).Column(col =>
            {
                // Summary cards
                col.Item()
                    .Background(AccentLight)
                    .Border(1).BorderColor(AccentBorder)
                    .Padding(10)
                    .Row(row =>
                    {
                        SummaryCard(row, "Total",          total,    Colors.Grey.Darken3);
                        SummaryCard(row, "Active",         active,   Colors.Green.Darken2);
                        SummaryCard(row, "Inactive",       inactive, Colors.Red.Darken2);
                        SummaryCard(row, "Root",           root,     Accent);
                        SummaryCard(row, "Sub-categories", sub,      Colors.Purple.Darken2);
                    });

                if (rows.Count == 0)
                {
                    col.Item().PaddingTop(40).AlignCenter()
                        .Text("No categories match the applied filters.")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                    return;
                }

                // Data table
                col.Item().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(25);   // #
                        c.RelativeColumn(2.5f); // Name EN
                        c.RelativeColumn(2f);   // Name BN
                        c.RelativeColumn(2f);   // Parent
                        c.ConstantColumn(50);   // Sub-cats
                        c.ConstantColumn(38);   // Sort
                        c.ConstantColumn(52);   // Status
                    });

                    table.Header(h =>
                    {
                        void HCell(string text) =>
                            h.Cell().Background(Accent).Padding(5).AlignMiddle()
                             .Text(text).FontSize(8.5f).Bold().FontColor(Colors.White);

                        HCell("#");
                        HCell("Name (English)");
                        HCell("Name (Bengali)");
                        HCell("Parent Category");
                        HCell("Sub-cats");
                        HCell("Sort");
                        HCell("Status");
                    });

                    for (var i = 0; i < rows.Count; i++)
                    {
                        var r  = rows[i];
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        void DCell(string? val, bool center = false)
                        {
                            var cell = table.Cell().Background(bg).Padding(4);
                            if (center)
                                cell.AlignCenter().AlignMiddle().Text(val ?? "—").FontSize(9);
                            else
                                cell.AlignMiddle().Text(val ?? "—").FontSize(9);
                        }

                        DCell((i + 1).ToString(), center: true);
                        DCell(r.Name);
                        DCell(r.NameBn);
                        DCell(r.ParentCategoryName);
                        DCell(r.ChildCount.ToString(), center: true);
                        DCell(r.SortOrder.ToString(), center: true);

                        var statusColor = r.Status == "Active"
                            ? Colors.Green.Darken2
                            : Colors.Red.Darken2;

                        table.Cell().Background(bg).Padding(4).AlignCenter().AlignMiddle()
                            .Text(r.Status).FontSize(8.5f).Bold().FontColor(statusColor);
                    }
                });
            });

            // ── Footer ─────────────────────────────────────────────────────────
            page.Footer()
                .BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2)
                .PaddingTop(6)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("BasicCommerce POS  |  Category Report")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);

                    row.ConstantItem(80).AlignRight().Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });

        })).GeneratePdf();
    }

    private static void SummaryCard(RowDescriptor row, string label, int value, string color)
    {
        row.RelativeItem().AlignCenter().Column(col =>
        {
            col.Item().Text(value.ToString())
                .FontSize(18).Bold().FontColor(color).AlignCenter();
            col.Item().PaddingTop(2).Text(label)
                .FontSize(8).FontColor(Colors.Grey.Darken2).AlignCenter();
        });
    }
}
