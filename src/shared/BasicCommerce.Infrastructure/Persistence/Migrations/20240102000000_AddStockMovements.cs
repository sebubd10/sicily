using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddStockMovements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "StockMovements",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                ProductId = t.Column<Guid>(nullable: false),
                Type = t.Column<string>(maxLength: 20, nullable: false),
                Quantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                QuantityBefore = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                QuantityAfter = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                RelatedStoreId = t.Column<Guid>(nullable: true),
                ReferenceTransactionId = t.Column<Guid>(nullable: true),
                Reference = t.Column<string>(maxLength: 100, nullable: true),
                Notes = t.Column<string>(maxLength: 500, nullable: true),
                RecordedByUserId = t.Column<Guid>(nullable: false),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_StockMovements", x => x.Id);
                t.ForeignKey("FK_StockMovements_Products", x => x.ProductId,
                    principalTable: "Products", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_StockMovements_Stores", x => x.StoreId,
                    principalTable: "Stores", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_StockMovements_TenantId",
            table: "StockMovements", column: "TenantId");
        migrationBuilder.CreateIndex(name: "IX_StockMovements_StoreId_ProductId",
            table: "StockMovements", columns: ["StoreId", "ProductId"]);
        migrationBuilder.CreateIndex(name: "IX_StockMovements_CreatedAt",
            table: "StockMovements", column: "CreatedAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "StockMovements");
    }
}
