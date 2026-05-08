using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddTillSessions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TillSessions",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                TerminalId = t.Column<Guid>(nullable: false),
                OpenedBy = t.Column<Guid>(nullable: false),
                ClosedBy = t.Column<Guid>(nullable: true),
                OpeningFloat = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                ClosingBalance = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                ClosingVariance = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                ExpectedClosingBalance = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                SessionStatus = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Open"),
                OpenedAt = t.Column<DateTime>(nullable: false),
                ClosedAt = t.Column<DateTime>(nullable: true),
                Notes = t.Column<string>(maxLength: 500, nullable: true),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_TillSessions", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TillSessions_TenantId_TerminalId_SessionStatus",
            table: "TillSessions",
            columns: ["TenantId", "TerminalId", "SessionStatus"]);

        migrationBuilder.CreateIndex(
            name: "IX_TillSessions_TenantId_StoreId",
            table: "TillSessions",
            columns: ["TenantId", "StoreId"]);

        migrationBuilder.CreateTable(
            name: "PettyTransactions",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TillSessionId = t.Column<Guid>(nullable: false),
                Type = t.Column<string>(maxLength: 20, nullable: false),
                Amount = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                Reason = t.Column<string>(maxLength: 200, nullable: false),
                PerformedBy = t.Column<Guid>(nullable: false),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_PettyTransactions", x => x.Id);
                t.ForeignKey("FK_PettyTransactions_TillSessions_TillSessionId",
                    x => x.TillSessionId, "TillSessions", "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PettyTransactions_TillSessionId",
            table: "PettyTransactions",
            column: "TillSessionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("PettyTransactions");
        migrationBuilder.DropTable("TillSessions");
    }
}
