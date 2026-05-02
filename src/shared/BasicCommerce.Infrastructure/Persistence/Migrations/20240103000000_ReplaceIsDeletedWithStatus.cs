using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class ReplaceIsDeletedWithStatus : Migration
{
    // Tables that had both IsActive and IsDeleted columns
    private static readonly string[] TablesWithIsActive =
        ["Tenants", "Stores", "Terminals", "Users", "Customers", "Categories",
         "Products", "VatRates", "CreditAccounts"];

    // Tables that only had IsDeleted (no separate IsActive)
    private static readonly string[] TablesWithoutIsActive =
        ["Transactions", "LineItems", "Payments", "StockMovements",
         "CreditTransactions", "UserRefreshTokens", "StockLevels"];

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add Status column to all entity tables
        foreach (var table in TablesWithIsActive.Concat(TablesWithoutIsActive))
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: table,
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Active");
        }

        // For tables with IsActive: migrate combined state into Status
        foreach (var table in TablesWithIsActive)
        {
            migrationBuilder.Sql(
                $"UPDATE [{table}] SET [Status] = 'Deleted' WHERE [IsDeleted] = 1");
            migrationBuilder.Sql(
                $"UPDATE [{table}] SET [Status] = 'Inactive' WHERE [IsDeleted] = 0 AND [IsActive] = 0");
        }

        // For tables with only IsDeleted
        foreach (var table in TablesWithoutIsActive)
        {
            migrationBuilder.Sql(
                $"UPDATE [{table}] SET [Status] = 'Deleted' WHERE [IsDeleted] = 1");
        }

        // Rename Transaction Status → TransactionStatus (was renamed in domain)
        migrationBuilder.RenameColumn(
            name: "Status",
            table: "Transactions",
            newName: "TransactionStatus");

        // Add index on Status for the most-queried tables
        foreach (var table in new[] { "Products", "Users", "Customers", "Transactions" })
        {
            migrationBuilder.CreateIndex(
                name: $"IX_{table}_Status",
                table: table,
                column: table == "Transactions" ? "TransactionStatus" : "Status");
        }

        // Drop IsDeleted columns
        foreach (var table in TablesWithIsActive.Concat(TablesWithoutIsActive))
        {
            migrationBuilder.DropColumn(name: "IsDeleted", table: table);
        }

        // Drop IsActive columns from tables that had them
        foreach (var table in TablesWithIsActive)
        {
            migrationBuilder.DropColumn(name: "IsActive", table: table);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Re-add IsDeleted and IsActive columns
        foreach (var table in TablesWithIsActive.Concat(TablesWithoutIsActive))
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: table,
                nullable: false,
                defaultValue: false);
        }

        foreach (var table in TablesWithIsActive)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: table,
                nullable: false,
                defaultValue: true);
        }

        // Restore data
        foreach (var table in TablesWithIsActive)
        {
            migrationBuilder.Sql(
                $"UPDATE [{table}] SET [IsDeleted] = 1 WHERE [Status] = 'Deleted'");
            migrationBuilder.Sql(
                $"UPDATE [{table}] SET [IsActive] = 0 WHERE [Status] = 'Inactive'");
        }

        foreach (var table in TablesWithoutIsActive)
        {
            migrationBuilder.Sql(
                $"UPDATE [{table}] SET [IsDeleted] = 1 WHERE [Status] = 'Deleted'");
        }

        // Rename TransactionStatus back to Status
        migrationBuilder.RenameColumn(
            name: "TransactionStatus",
            table: "Transactions",
            newName: "Status");

        // Drop Status columns
        foreach (var table in TablesWithIsActive.Concat(TablesWithoutIsActive))
        {
            migrationBuilder.DropColumn(name: "Status", table: table);
        }

        // Drop indexes
        foreach (var table in new[] { "Products", "Users", "Customers" })
        {
            migrationBuilder.DropIndex(name: $"IX_{table}_Status", table: table);
        }
        migrationBuilder.DropIndex(name: "IX_Transactions_Status", table: "Transactions");
    }
}
