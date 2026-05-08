using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddStockBatches : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsPerishable",
            table: "Products",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "StockBatches",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                ProductId = t.Column<Guid>(nullable: false),
                LotNumber = t.Column<string>(maxLength: 100, nullable: true),
                ExpiryDate = t.Column<DateTime>(nullable: true),
                ReceivedQuantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                RemainingQuantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                UnitCost = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                PurchaseOrderId = t.Column<Guid>(nullable: true),
                IsExpired = t.Column<bool>(nullable: false, defaultValue: false),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_StockBatches", x => x.Id);
                t.ForeignKey("FK_StockBatches_Products_ProductId",
                    x => x.ProductId, "Products", "Id",
                    onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_StockBatches_Stores_StoreId",
                    x => x.StoreId, "Stores", "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_StockBatches_TenantId_StoreId_ProductId_IsExpired",
            table: "StockBatches",
            columns: ["TenantId", "StoreId", "ProductId", "IsExpired"]);

        migrationBuilder.CreateIndex(
            name: "IX_StockBatches_ExpiryDate",
            table: "StockBatches",
            column: "ExpiryDate");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("StockBatches");
        migrationBuilder.DropColumn(name: "IsPerishable", table: "Products");
    }
}
