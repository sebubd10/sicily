using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddSupplierCatalogue : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SupplierProducts",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                SupplierId = t.Column<Guid>(nullable: false),
                ProductId = t.Column<Guid>(nullable: false),
                SupplierSku = t.Column<string>(maxLength: 100, nullable: true),
                UnitCost = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                CurrencyCode = t.Column<string>(maxLength: 3, nullable: false, defaultValue: "BDT"),
                MinOrderQuantity = t.Column<int>(nullable: true),
                LeadTimeDays = t.Column<int>(nullable: true),
                Notes = t.Column<string>(maxLength: 500, nullable: true),
                PriceLastConfirmedAt = t.Column<DateTime>(nullable: false),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_SupplierProducts", x => x.Id);
                t.ForeignKey(
                    name: "FK_SupplierProducts_Suppliers_SupplierId",
                    column: x => x.SupplierId,
                    principalTable: "Suppliers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                t.ForeignKey(
                    name: "FK_SupplierProducts_Products_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SupplierProducts_TenantId_SupplierId_ProductId",
            table: "SupplierProducts",
            columns: ["TenantId", "SupplierId", "ProductId"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SupplierProducts_TenantId_ProductId",
            table: "SupplierProducts",
            columns: ["TenantId", "ProductId"]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("SupplierProducts");
    }
}
