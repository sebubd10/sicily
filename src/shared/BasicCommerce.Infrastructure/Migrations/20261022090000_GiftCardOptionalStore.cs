using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicCommerce.Infrastructure.Migrations
{
    /// <summary>
    /// Gift cards can now be issued without a store, which makes them usable at every store.
    /// </summary>
    public partial class GiftCardOptionalStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "StoreId",
                table: "GiftCards",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Store-less cards can't be represented any more; this fails if any exist,
            // rather than silently assigning them to an arbitrary store.
            migrationBuilder.AlterColumn<Guid>(
                name: "StoreId",
                table: "GiftCards",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
