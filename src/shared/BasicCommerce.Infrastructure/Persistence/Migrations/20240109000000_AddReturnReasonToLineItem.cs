using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddReturnReasonToLineItem : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ReturnReason",
            table: "LineItems",
            maxLength: 30,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DamageDisposition",
            table: "LineItems",
            maxLength: 20,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ReturnReason", table: "LineItems");
        migrationBuilder.DropColumn(name: "DamageDisposition", table: "LineItems");
    }
}
