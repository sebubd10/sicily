using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddPromotions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Promotions",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 200, nullable: false),
                Description = t.Column<string>(maxLength: 1000, nullable: true),
                Type = t.Column<string>(maxLength: 30, nullable: false),
                PromotionStatus = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Draft"),
                ProductId = t.Column<Guid>(nullable: true),
                CategoryId = t.Column<Guid>(nullable: true),
                StoreId = t.Column<Guid>(nullable: true),
                DiscountPercentage = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                DiscountAmount = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                BuyQuantity = t.Column<int>(nullable: true),
                GetQuantity = t.Column<int>(nullable: true),
                MinimumCartValue = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                CouponCode = t.Column<string>(maxLength: 50, nullable: true),
                RequiresCoupon = t.Column<bool>(nullable: false, defaultValue: false),
                StartsAt = t.Column<DateTime>(nullable: true),
                EndsAt = t.Column<DateTime>(nullable: true),
                MaxUses = t.Column<int>(nullable: true),
                UsedCount = t.Column<int>(nullable: false, defaultValue: 0),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Promotions", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Promotions_TenantId_PromotionStatus",
            table: "Promotions",
            columns: ["TenantId", "PromotionStatus"]);

        migrationBuilder.CreateIndex(
            name: "IX_Promotions_TenantId_CouponCode",
            table: "Promotions",
            columns: ["TenantId", "CouponCode"],
            unique: true,
            filter: "\"CouponCode\" IS NOT NULL");

        migrationBuilder.AddColumn<Guid>(
            name: "AppliedPromotionId",
            table: "LineItems",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AppliedPromotionName",
            table: "LineItems",
            maxLength: 200,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("AppliedPromotionId", "LineItems");
        migrationBuilder.DropColumn("AppliedPromotionName", "LineItems");
        migrationBuilder.DropTable("Promotions");
    }
}
