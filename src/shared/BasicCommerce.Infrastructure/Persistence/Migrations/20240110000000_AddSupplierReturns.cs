using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddSupplierReturns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SupplierReturns",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                ReturnNumber = t.Column<string>(maxLength: 50, nullable: false),
                SupplierId = t.Column<Guid>(nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                PurchaseOrderId = t.Column<Guid>(nullable: true),
                ReturnStatus = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Draft"),
                Notes = t.Column<string>(maxLength: 1000, nullable: true),
                ExpectedCreditAmount = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                ActualCreditAmount = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                CreditNoteReference = t.Column<string>(maxLength: 100, nullable: true),
                ShippedAt = t.Column<DateTime>(nullable: true),
                CreditReceivedAt = t.Column<DateTime>(nullable: true),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_SupplierReturns", x => x.Id);
                t.ForeignKey("FK_SupplierReturns_Suppliers_SupplierId",
                    x => x.SupplierId, "Suppliers", "Id",
                    onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_SupplierReturns_Stores_StoreId",
                    x => x.StoreId, "Stores", "Id",
                    onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_SupplierReturns_PurchaseOrders_PurchaseOrderId",
                    x => x.PurchaseOrderId, "PurchaseOrders", "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SupplierReturns_ReturnNumber",
            table: "SupplierReturns",
            column: "ReturnNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_SupplierReturns_TenantId_SupplierId_ReturnStatus",
            table: "SupplierReturns",
            columns: ["TenantId", "SupplierId", "ReturnStatus"]);

        migrationBuilder.CreateTable(
            name: "SupplierReturnItems",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                SupplierReturnId = t.Column<Guid>(nullable: false),
                ProductId = t.Column<Guid>(nullable: false),
                Quantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                UnitCost = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                Reason = t.Column<string>(maxLength: 30, nullable: false),
                Notes = t.Column<string>(maxLength: 500, nullable: true),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_SupplierReturnItems", x => x.Id);
                t.ForeignKey("FK_SupplierReturnItems_SupplierReturns_SupplierReturnId",
                    x => x.SupplierReturnId, "SupplierReturns", "Id",
                    onDelete: ReferentialAction.Cascade);
                t.ForeignKey("FK_SupplierReturnItems_Products_ProductId",
                    x => x.ProductId, "Products", "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SupplierReturnItems_SupplierReturnId",
            table: "SupplierReturnItems",
            column: "SupplierReturnId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("SupplierReturnItems");
        migrationBuilder.DropTable("SupplierReturns");
    }
}
