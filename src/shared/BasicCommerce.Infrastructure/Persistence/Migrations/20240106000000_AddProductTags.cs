using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddProductTags : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("ProductTags", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            Name = t.Column<string>(maxLength: 100, nullable: false),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t => t.PrimaryKey("PK_ProductTags", x => x.Id));

        migrationBuilder.CreateIndex("IX_ProductTags_TenantId_Name", "ProductTags",
            ["TenantId", "Name"], unique: true);

        migrationBuilder.CreateTable("ProductTagAssignments", t => new
        {
            ProductId = t.Column<Guid>(nullable: false),
            ProductTagId = t.Column<Guid>(nullable: false)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_ProductTagAssignments", x => new { x.ProductId, x.ProductTagId });
            t.ForeignKey("FK_ProductTagAssignments_Products_ProductId",
                x => x.ProductId, "Products", "Id", onDelete: ReferentialAction.Cascade);
            t.ForeignKey("FK_ProductTagAssignments_ProductTags_ProductTagId",
                x => x.ProductTagId, "ProductTags", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_ProductTagAssignments_ProductTagId",
            "ProductTagAssignments", "ProductTagId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ProductTagAssignments");
        migrationBuilder.DropTable("ProductTags");
    }
}
