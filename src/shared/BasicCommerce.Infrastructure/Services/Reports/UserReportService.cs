using BasicCommerce.Application.Interfaces;
using BasicCommerce.Contracts.Users;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BasicCommerce.Infrastructure.Services.Reports;

public sealed class UserReportService : IUserReportService
{
    private const string Accent       = "#1565C0";
    private const string AccentLight  = "#E3F2FD";
    private const string AccentBorder = "#BBDEFB";
    private const string FilterBg    = "#FFF8E1";
    private const string FilterBorder = "#FFE082";

    private static readonly Dictionary<string, string> RoleColors = new()
    {
        ["SystemAdmin"]  = "#B71C1C",
        ["ChainAdmin"]   = "#4A148C",
        ["StoreManager"] = "#1565C0",
        ["Supervisor"]   = "#E65100",
        ["Cashier"]      = "#2E7D32",
    };

    static UserReportService()
        => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Generate(IEnumerable<UserReportRow> users, string? search = null)
    {
        var rows = users.ToList();
        var total    = rows.Count;
        var active   = rows.Count(r => r.Status == "Active");
        var inactive = total - active;
        var locked   = rows.Count(r => r.IsLocked);
        var hasFilter = !string.IsNullOrWhiteSpace(search);

        return Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(1.8f, Unit.Centimetre);
            page.DefaultTextStyle(t => t.FontFamily(Fonts.Arial).FontSize(9));

            // ── Header ──────────────────────────────────────────────────────────
            page.Header().Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("User Report")
                            .FontSize(22).Bold().FontColor(Accent);
                        c.Item().Text("BasicCommerce POS")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                    });

                    row.ConstantItem(220).AlignRight().Column(c =>
                    {
                        c.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy  HH:mm}")
                            .FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                        if (rows.Count == 0)
                            c.Item().PaddingTop(2)
                                .Text("No users found")
                                .FontSize(8).FontColor(Colors.Red.Darken2);
                    });
                });

                if (hasFilter)
                {
                    col.Item().PaddingTop(6)
                        .Background(FilterBg)
                        .Border(0.5f).BorderColor(FilterBorder)
                        .Padding(6)
                        .Row(row =>
                        {
                            row.AutoItem().Text("Filters applied: ")
                                .FontSize(8).SemiBold().FontColor(Colors.Orange.Darken3);
                            row.AutoItem().PaddingLeft(4)
                                .Text($"Search = \"{search}\"")
                                .FontSize(8).FontColor(Colors.Orange.Darken3);
                        });
                }

                col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(Accent);
            });

            // ── Content ─────────────────────────────────────────────────────────
            page.Content().PaddingVertical(12).Column(col =>
            {
                // Summary cards
                col.Item()
                    .Background(AccentLight)
                    .Border(1).BorderColor(AccentBorder)
                    .Padding(10)
                    .Row(row =>
                    {
                        SummaryCard(row, "Total",    total,    Colors.Grey.Darken3);
                        SummaryCard(row, "Active",   active,   Colors.Green.Darken2);
                        SummaryCard(row, "Inactive", inactive, Colors.Red.Darken2);
                        SummaryCard(row, "Locked",   locked,   Colors.Orange.Darken3);
                    });

                if (rows.Count == 0)
                {
                    col.Item().PaddingTop(40).AlignCenter()
                        .Text("No users match the applied filters.")
                        .FontSize(12).FontColor(Colors.Grey.Darken1);
                    return;
                }

                // Data table
                col.Item().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(25);   // #
                        c.ConstantColumn(62);   // Code
                        c.RelativeColumn(2.2f); // Full Name
                        c.RelativeColumn(2.5f); // Email
                        c.ConstantColumn(75);   // Role
                        c.RelativeColumn(1.6f); // User Type
                        c.RelativeColumn(1.6f); // Store
                        c.ConstantColumn(52);   // Status
                        c.ConstantColumn(80);   // Last Login
                    });

                    table.Header(h =>
                    {
                        void HCell(string text) =>
                            h.Cell().Background(Accent).Padding(5).AlignMiddle()
                             .Text(text).FontSize(8.5f).Bold().FontColor(Colors.White);

                        HCell("#");
                        HCell("Code");
                        HCell("Full Name");
                        HCell("Email");
                        HCell("Role");
                        HCell("User Type");
                        HCell("Store");
                        HCell("Status");
                        HCell("Last Login");
                    });

                    for (var i = 0; i < rows.Count; i++)
                    {
                        var r  = rows[i];
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        void DCell(string? val, bool center = false, bool mono = false)
                        {
                            var cell = table.Cell().Background(bg).Padding(4);
                            var txt  = center
                                ? cell.AlignCenter().AlignMiddle().Text(val ?? "—").FontSize(8.5f)
                                : cell.AlignMiddle().Text(val ?? "—").FontSize(8.5f);
                            if (mono) txt.FontFamily(Fonts.Courier);
                        }

                        DCell((i + 1).ToString(), center: true);
                        DCell(r.EmployeeCode, mono: true);
                        DCell(r.FullName);
                        DCell(r.Email);

                        // Role — coloured
                        var roleColor = RoleColors.GetValueOrDefault(r.Role, Colors.Grey.Darken2);
                        table.Cell().Background(bg).Padding(4).AlignMiddle()
                            .Text(r.Role).FontSize(8.5f).Bold().FontColor(roleColor);

                        DCell(r.UserTypeName);
                        DCell(r.StoreName);

                        // Status + locked badge
                        var statusColor = r.Status == "Active"
                            ? Colors.Green.Darken2
                            : Colors.Red.Darken2;
                        table.Cell().Background(bg).Padding(4).AlignCenter().AlignMiddle()
                            .Column(sc =>
                            {
                                sc.Item().Text(r.Status).FontSize(8.5f).Bold().FontColor(statusColor);
                                if (r.IsLocked)
                                    sc.Item().Text("Locked").FontSize(7).FontColor(Colors.Orange.Darken3);
                            });

                        DCell(r.LastLoginAt.HasValue
                            ? r.LastLoginAt.Value.ToString("dd MMM yyyy")
                            : "Never", center: true);
                    }
                });
            });

            // ── Footer ──────────────────────────────────────────────────────────
            page.Footer()
                .BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2)
                .PaddingTop(6)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("BasicCommerce POS  |  User Report")
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
