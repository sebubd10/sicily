using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddProductReviews : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("ProductReviews", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            ProductId = t.Column<Guid>(nullable: false),
            CustomerId = t.Column<Guid>(nullable: true),
            StoreId = t.Column<Guid>(nullable: true),
            CustomerName = t.Column<string>(maxLength: 200, nullable: false),
            Title = t.Column<string>(maxLength: 300, nullable: false),
            ReviewText = t.Column<string>(maxLength: 3000, nullable: false),
            Rating = t.Column<int>(nullable: false),
            IsApproved = t.Column<bool>(nullable: false, defaultValue: false),
            IsVerifiedPurchase = t.Column<bool>(nullable: false, defaultValue: false),
            HelpfulYesTotal = t.Column<int>(nullable: false, defaultValue: 0),
            HelpfulNoTotal = t.Column<int>(nullable: false, defaultValue: 0),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_ProductReviews", x => x.Id);
            t.ForeignKey("FK_ProductReviews_Products_ProductId",
                x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
            t.ForeignKey("FK_ProductReviews_Customers_CustomerId",
                x => x.CustomerId, "Customers", "Id", onDelete: ReferentialAction.SetNull);
        });

        migrationBuilder.CreateIndex("IX_ProductReviews_TenantId_ProductId_CreatedAt",
            "ProductReviews", ["TenantId", "ProductId", "CreatedAt"]);
        migrationBuilder.CreateIndex("IX_ProductReviews_TenantId_CustomerId",
            "ProductReviews", ["TenantId", "CustomerId"]);

        migrationBuilder.CreateTable("ProductReviewDetails", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            ProductReviewId = t.Column<Guid>(nullable: false),
            Comment = t.Column<string>(maxLength: 2000, nullable: false),
            IsAdminReply = t.Column<bool>(nullable: false, defaultValue: false),
            UserId = t.Column<Guid>(nullable: true),
            CommenterName = t.Column<string>(maxLength: 200, nullable: true),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_ProductReviewDetails", x => x.Id);
            t.ForeignKey("FK_ProductReviewDetails_ProductReviews_ProductReviewId",
                x => x.ProductReviewId, "ProductReviews", "Id",
                onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_ProductReviewDetails_ProductReviewId_CreatedAt",
            "ProductReviewDetails", ["ProductReviewId", "CreatedAt"]);

        migrationBuilder.CreateTable("ProductReviewHelpfulnesses", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            ProductReviewId = t.Column<Guid>(nullable: false),
            CustomerId = t.Column<Guid>(nullable: false),
            IsHelpful = t.Column<bool>(nullable: false),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_ProductReviewHelpfulnesses", x => x.Id);
            t.ForeignKey("FK_ProductReviewHelpfulnesses_ProductReviews_ProductReviewId",
                x => x.ProductReviewId, "ProductReviews", "Id",
                onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_ProductReviewHelpfulnesses_ReviewId_CustomerId",
            "ProductReviewHelpfulnesses",
            ["ProductReviewId", "CustomerId"], unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ProductReviewHelpfulnesses");
        migrationBuilder.DropTable("ProductReviewDetails");
        migrationBuilder.DropTable("ProductReviews");
    }
}
