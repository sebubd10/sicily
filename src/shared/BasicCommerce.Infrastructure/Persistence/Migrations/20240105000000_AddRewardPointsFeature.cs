using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddRewardPointsFeature : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("RewardPointsSettings", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            ExchangeRate = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 1m),
            MinimumPointsToUse = t.Column<int>(nullable: false, defaultValue: 0),
            MaximumPointsPerOrder = t.Column<int>(nullable: false, defaultValue: 0),
            MaximumRedeemedRate = t.Column<decimal>(precision: 5, scale: 4, nullable: false, defaultValue: 0m),
            PurchaseSpendPerPoint = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 10m),
            PointsEarnedPerSpend = t.Column<int>(nullable: false, defaultValue: 1),
            PurchasePointsValidityDays = t.Column<int>(nullable: false, defaultValue: 45),
            MinimumOrderTotalForPoints = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
            PointsForRegistration = t.Column<int>(nullable: false, defaultValue: 0),
            RegistrationPointsValidityDays = t.Column<int>(nullable: false, defaultValue: 30),
            ActivatePointsImmediately = t.Column<bool>(nullable: false, defaultValue: true),
            DisplayHowMuchWillBeEarned = t.Column<bool>(nullable: false, defaultValue: true),
            PointsAccumulatedForAllStores = t.Column<bool>(nullable: false, defaultValue: true),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t => t.PrimaryKey("PK_RewardPointsSettings", x => x.Id));

        migrationBuilder.CreateIndex("IX_RewardPointsSettings_TenantId", "RewardPointsSettings", "TenantId", unique: true);

        migrationBuilder.CreateTable("RewardPointsAccounts", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            CustomerId = t.Column<Guid>(nullable: false),
            StoreId = t.Column<Guid>(nullable: true),
            TotalEarnedPoints = t.Column<int>(nullable: false, defaultValue: 0),
            UsedPoints = t.Column<int>(nullable: false, defaultValue: 0),
            ExpiredPoints = t.Column<int>(nullable: false, defaultValue: 0),
            PendingPoints = t.Column<int>(nullable: false, defaultValue: 0),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_RewardPointsAccounts", x => x.Id);
            t.ForeignKey("FK_RewardPointsAccounts_Customers_CustomerId", x => x.CustomerId,
                "Customers", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_RewardPointsAccounts_TenantId_CustomerId_StoreId",
            "RewardPointsAccounts", ["TenantId", "CustomerId", "StoreId"], unique: true);

        migrationBuilder.CreateTable("RewardPointsEntries", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            RewardPointsAccountId = t.Column<Guid>(nullable: false),
            Points = t.Column<int>(nullable: false),
            EntryType = t.Column<string>(maxLength: 30, nullable: false),
            IsActivated = t.Column<bool>(nullable: false, defaultValue: true),
            ExpiresAt = t.Column<DateTime>(nullable: true),
            TransactionId = t.Column<Guid>(nullable: true),
            Notes = t.Column<string>(maxLength: 500, nullable: true),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_RewardPointsEntries", x => x.Id);
            t.ForeignKey("FK_RewardPointsEntries_RewardPointsAccounts_RewardPointsAccountId",
                x => x.RewardPointsAccountId, "RewardPointsAccounts", "Id",
                onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateIndex("IX_RewardPointsEntries_AccountId_CreatedAt",
            "RewardPointsEntries", ["RewardPointsAccountId", "CreatedAt"]);
        migrationBuilder.CreateIndex("IX_RewardPointsEntries_TransactionId",
            "RewardPointsEntries", "TransactionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("RewardPointsEntries");
        migrationBuilder.DropTable("RewardPointsAccounts");
        migrationBuilder.DropTable("RewardPointsSettings");
    }
}
