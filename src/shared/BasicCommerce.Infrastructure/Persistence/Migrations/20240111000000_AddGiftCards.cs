using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddGiftCards : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GiftCards",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Code = t.Column<string>(maxLength: 50, nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                InitialBalance = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                Balance = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                CardStatus = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Inactive"),
                ExpiryDate = t.Column<DateTime>(nullable: true),
                IssuedToCustomerId = t.Column<Guid>(nullable: true),
                IssuedInTransactionId = t.Column<Guid>(nullable: true),
                Notes = t.Column<string>(maxLength: 500, nullable: true),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_GiftCards", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GiftCards_TenantId_Code",
            table: "GiftCards",
            columns: ["TenantId", "Code"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_GiftCards_TenantId_StoreId_CardStatus",
            table: "GiftCards",
            columns: ["TenantId", "StoreId", "CardStatus"]);

        migrationBuilder.CreateTable(
            name: "GiftCardTransactions",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                GiftCardId = t.Column<Guid>(nullable: false),
                TransactionType = t.Column<string>(maxLength: 20, nullable: false),
                Amount = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                BalanceAfter = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                SaleTransactionId = t.Column<Guid>(nullable: true),
                Notes = t.Column<string>(maxLength: 500, nullable: true),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_GiftCardTransactions", x => x.Id);
                t.ForeignKey("FK_GiftCardTransactions_GiftCards_GiftCardId",
                    x => x.GiftCardId, "GiftCards", "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GiftCardTransactions_GiftCardId",
            table: "GiftCardTransactions",
            column: "GiftCardId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("GiftCardTransactions");
        migrationBuilder.DropTable("GiftCards");
    }
}
