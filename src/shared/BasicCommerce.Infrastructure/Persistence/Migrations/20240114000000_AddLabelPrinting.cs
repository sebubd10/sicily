using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddLabelPrinting : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LabelTemplates",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 200, nullable: false),
                Description = t.Column<string>(maxLength: 1000, nullable: true),
                LabelType = t.Column<string>(maxLength: 30, nullable: false),
                WidthMm = t.Column<decimal>(precision: 6, scale: 2, nullable: false),
                HeightMm = t.Column<decimal>(precision: 6, scale: 2, nullable: false),
                PrinterDpi = t.Column<int>(nullable: false, defaultValue: 203),
                LayoutJson = t.Column<string>(type: "nvarchar(max)", nullable: false),
                DefaultBarcodeSymbology = t.Column<string>(maxLength: 20, nullable: false),
                IsDefault = t.Column<bool>(nullable: false, defaultValue: false),
                SortOrder = t.Column<int>(nullable: false, defaultValue: 0),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_LabelTemplates", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LabelTemplates_TenantId_LabelType",
            table: "LabelTemplates",
            columns: ["TenantId", "LabelType"]);

        migrationBuilder.CreateIndex(
            name: "IX_LabelTemplates_TenantId_Name",
            table: "LabelTemplates",
            columns: ["TenantId", "Name"]);

        migrationBuilder.CreateTable(
            name: "LabelPrintJobs",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                JobNumber = t.Column<string>(maxLength: 30, nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                TemplateId = t.Column<Guid>(nullable: false),
                OutputFormat = t.Column<string>(maxLength: 20, nullable: false),
                JobStatus = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Pending"),
                PrinterName = t.Column<string>(maxLength: 200, nullable: true),
                Notes = t.Column<string>(maxLength: 500, nullable: true),
                CreatedByUserId = t.Column<Guid>(nullable: false),
                PrintedByUserId = t.Column<Guid>(nullable: true),
                PrintedAt = t.Column<DateTime>(nullable: true),
                FailureReason = t.Column<string>(maxLength: 1000, nullable: true),
                TotalLabels = t.Column<int>(nullable: false, defaultValue: 0),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_LabelPrintJobs", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LabelPrintJobs_JobNumber",
            table: "LabelPrintJobs",
            column: "JobNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LabelPrintJobs_TenantId_StoreId_JobStatus",
            table: "LabelPrintJobs",
            columns: ["TenantId", "StoreId", "JobStatus"]);

        migrationBuilder.CreateTable(
            name: "LabelPrintJobItems",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                PrintJobId = t.Column<Guid>(nullable: false),
                ProductId = t.Column<Guid>(nullable: false),
                Quantity = t.Column<int>(nullable: false),
                OverridePrice = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                CustomText = t.Column<string>(maxLength: 200, nullable: true),
                LotNumber = t.Column<string>(maxLength: 100, nullable: true),
                ExpiryDate = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_LabelPrintJobItems", x => x.Id);
                t.ForeignKey(
                    name: "FK_LabelPrintJobItems_LabelPrintJobs_PrintJobId",
                    column: x => x.PrintJobId,
                    principalTable: "LabelPrintJobs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LabelPrintJobItems_PrintJobId",
            table: "LabelPrintJobItems",
            column: "PrintJobId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("LabelPrintJobItems");
        migrationBuilder.DropTable("LabelPrintJobs");
        migrationBuilder.DropTable("LabelTemplates");
    }
}
