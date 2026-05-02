using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── Tenants ────────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 200, nullable: false),
                Slug = t.Column<string>(maxLength: 100, nullable: false),
                ContactEmail = t.Column<string>(maxLength: 256, nullable: false),
                ContactPhone = t.Column<string>(maxLength: 20, nullable: true),
                BusinessIdentificationNumber = t.Column<string>(maxLength: 50, nullable: true),
                VatRegistrationNumber = t.Column<string>(maxLength: 50, nullable: true),
                CurrencyCode = t.Column<string>(maxLength: 3, nullable: false, defaultValue: "BDT"),
                DefaultLanguage = t.Column<string>(maxLength: 10, nullable: false, defaultValue: "en"),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t => t.PrimaryKey("PK_Tenants", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_Tenants_Slug",
            table: "Tenants", column: "Slug", unique: true);

        // ── Stores ─────────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Stores",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 200, nullable: false),
                Code = t.Column<string>(maxLength: 20, nullable: false),
                Address_Line1 = t.Column<string>(maxLength: 200, nullable: false),
                Address_Line2 = t.Column<string>(maxLength: 200, nullable: true),
                Address_City = t.Column<string>(maxLength: 100, nullable: false),
                Address_District = t.Column<string>(maxLength: 100, nullable: false),
                Address_PostalCode = t.Column<string>(maxLength: 20, nullable: false),
                Address_Country = t.Column<string>(maxLength: 5, nullable: false, defaultValue: "BD"),
                Phone = t.Column<string>(maxLength: 20, nullable: true),
                Email = t.Column<string>(maxLength: 256, nullable: true),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                OpeningTime = t.Column<TimeOnly>(nullable: false),
                ClosingTime = t.Column<TimeOnly>(nullable: false),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Stores", x => x.Id);
                t.ForeignKey("FK_Stores_Tenants", x => x.TenantId,
                    principalTable: "Tenants", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_Stores_TenantId_Code",
            table: "Stores", columns: ["TenantId", "Code"], unique: true);

        // ── Terminals ──────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Terminals",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 100, nullable: false),
                Code = t.Column<string>(maxLength: 20, nullable: false),
                Type = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Standard"),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Offline"),
                CurrentCashierId = t.Column<Guid>(nullable: true),
                OpeningFloat = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                LastActivityAt = t.Column<DateTime>(nullable: true),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Terminals", x => x.Id);
                t.ForeignKey("FK_Terminals_Stores", x => x.StoreId,
                    principalTable: "Stores", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_Terminals_StoreId",
            table: "Terminals", column: "StoreId");

        // ── Users ──────────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Users",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                EmployeeCode = t.Column<string>(maxLength: 20, nullable: false),
                FirstName = t.Column<string>(maxLength: 100, nullable: false),
                LastName = t.Column<string>(maxLength: 100, nullable: false),
                Email = t.Column<string>(maxLength: 256, nullable: false),
                PasswordHash = t.Column<string>(maxLength: 500, nullable: true),
                Role = t.Column<string>(maxLength: 30, nullable: false),
                StoreId = t.Column<Guid>(nullable: true),
                PhoneNumber = t.Column<string>(maxLength: 20, nullable: true),
                GoogleId = t.Column<string>(maxLength: 200, nullable: true),
                MicrosoftId = t.Column<string>(maxLength: 200, nullable: true),
                AuthProvider = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Local"),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                LastLoginAt = t.Column<DateTime>(nullable: true),
                FailedLoginAttempts = t.Column<int>(nullable: false, defaultValue: 0),
                LockedUntil = t.Column<DateTime>(nullable: true),
                PreferredLanguage = t.Column<string>(maxLength: 10, nullable: false, defaultValue: "en"),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t => t.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_Users_Email",
            table: "Users", column: "Email", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Users_TenantId_EmployeeCode",
            table: "Users", columns: ["TenantId", "EmployeeCode"], unique: true);
        migrationBuilder.CreateIndex(name: "IX_Users_GoogleId",
            table: "Users", column: "GoogleId", unique: true, filter: "\"GoogleId\" IS NOT NULL");
        migrationBuilder.CreateIndex(name: "IX_Users_MicrosoftId",
            table: "Users", column: "MicrosoftId", unique: true, filter: "\"MicrosoftId\" IS NOT NULL");

        // ── UserRefreshTokens ──────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "UserRefreshTokens",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                UserId = t.Column<Guid>(nullable: false),
                Token = t.Column<string>(maxLength: 500, nullable: false),
                ExpiresAt = t.Column<DateTime>(nullable: false),
                IsRevoked = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedByIp = t.Column<string>(maxLength: 45, nullable: true),
                RevokedByIp = t.Column<string>(maxLength: 45, nullable: true),
                RevokedAt = t.Column<DateTime>(nullable: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_UserRefreshTokens", x => x.Id);
                t.ForeignKey("FK_UserRefreshTokens_Users", x => x.UserId,
                    principalTable: "Users", principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_UserRefreshTokens_Token",
            table: "UserRefreshTokens", column: "Token", unique: true);
        migrationBuilder.CreateIndex(name: "IX_UserRefreshTokens_UserId",
            table: "UserRefreshTokens", column: "UserId");

        // ── Customers ──────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Customers",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Code = t.Column<string>(maxLength: 50, nullable: false),
                Name = t.Column<string>(maxLength: 200, nullable: false),
                Email = t.Column<string>(maxLength: 256, nullable: true),
                Phone = t.Column<string>(maxLength: 20, nullable: true),
                Address_Line1 = t.Column<string>(maxLength: 200, nullable: true),
                Address_Line2 = t.Column<string>(maxLength: 200, nullable: true),
                Address_City = t.Column<string>(maxLength: 100, nullable: true),
                Address_District = t.Column<string>(maxLength: 100, nullable: true),
                Address_PostalCode = t.Column<string>(maxLength: 20, nullable: true),
                Address_Country = t.Column<string>(maxLength: 5, nullable: true),
                CreditLimit = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                CurrentBalance = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                LoyaltyPoints = t.Column<int>(nullable: false, defaultValue: 0),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t => t.PrimaryKey("PK_Customers", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_Customers_TenantId_Code",
            table: "Customers", columns: ["TenantId", "Code"], unique: true);

        // ── VatRates ───────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "VatRates",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 100, nullable: false),
                Code = t.Column<string>(maxLength: 20, nullable: false),
                Rate = t.Column<decimal>(precision: 5, scale: 2, nullable: false),
                IsDefault = t.Column<bool>(nullable: false, defaultValue: false),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t => t.PrimaryKey("PK_VatRates", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_VatRates_TenantId_Code",
            table: "VatRates", columns: ["TenantId", "Code"], unique: true);

        // ── Categories ─────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Categories",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 200, nullable: false),
                NameBn = t.Column<string>(maxLength: 200, nullable: false),
                Description = t.Column<string>(maxLength: 1000, nullable: true),
                ParentCategoryId = t.Column<Guid>(nullable: true),
                SortOrder = t.Column<int>(nullable: false, defaultValue: 0),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                ImageUrl = t.Column<string>(maxLength: 500, nullable: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Categories", x => x.Id);
                t.ForeignKey("FK_Categories_ParentCategory", x => x.ParentCategoryId,
                    principalTable: "Categories", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_Categories_TenantId",
            table: "Categories", column: "TenantId");
        migrationBuilder.CreateIndex(name: "IX_Categories_ParentCategoryId",
            table: "Categories", column: "ParentCategoryId");

        // ── Products ───────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Products",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Sku = t.Column<string>(maxLength: 50, nullable: false),
                Barcode = t.Column<string>(maxLength: 100, nullable: false),
                Plu = t.Column<string>(maxLength: 20, nullable: true),
                Name = t.Column<string>(maxLength: 200, nullable: false),
                NameBn = t.Column<string>(maxLength: 200, nullable: false),
                Description = t.Column<string>(maxLength: 1000, nullable: true),
                CategoryId = t.Column<Guid>(nullable: false),
                Price = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                Currency = t.Column<string>(maxLength: 3, nullable: false, defaultValue: "BDT"),
                CostPrice = t.Column<decimal>(precision: 18, scale: 4, nullable: true),
                CostCurrency = t.Column<string>(maxLength: 3, nullable: true),
                UnitType = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Each"),
                UnitLabel = t.Column<string>(maxLength: 20, nullable: true),
                VatRateId = t.Column<Guid>(nullable: false),
                IsWeightBased = t.Column<bool>(nullable: false, defaultValue: false),
                IsAgeRestricted = t.Column<bool>(nullable: false, defaultValue: false),
                AgeRestrictionYears = t.Column<int>(nullable: true),
                IsEbtEligible = t.Column<bool>(nullable: false, defaultValue: false),
                TrackInventory = t.Column<bool>(nullable: false, defaultValue: true),
                ReorderLevel = t.Column<int>(nullable: false, defaultValue: 10),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                ImageUrl = t.Column<string>(maxLength: 500, nullable: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Products", x => x.Id);
                t.ForeignKey("FK_Products_Categories", x => x.CategoryId,
                    principalTable: "Categories", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                t.ForeignKey("FK_Products_VatRates", x => x.VatRateId,
                    principalTable: "VatRates", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_Products_Barcode",
            table: "Products", column: "Barcode", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Products_TenantId_Sku",
            table: "Products", columns: ["TenantId", "Sku"], unique: true);
        migrationBuilder.CreateIndex(name: "IX_Products_Plu",
            table: "Products", column: "Plu");

        // ── StockLevels ────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "StockLevels",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                ProductId = t.Column<Guid>(nullable: false),
                Quantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                ReservedQuantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                LowStockThreshold = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 10m),
                LastCountedAt = t.Column<DateTime>(nullable: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_StockLevels", x => x.Id);
                t.ForeignKey("FK_StockLevels_Products", x => x.ProductId,
                    principalTable: "Products", principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                t.ForeignKey("FK_StockLevels_Stores", x => x.StoreId,
                    principalTable: "Stores", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_StockLevels_TenantId_StoreId_ProductId",
            table: "StockLevels", columns: ["TenantId", "StoreId", "ProductId"], unique: true);

        // ── Transactions ───────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                TransactionNumber = t.Column<string>(maxLength: 50, nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                TerminalId = t.Column<Guid>(nullable: false),
                CashierId = t.Column<Guid>(nullable: false),
                CustomerId = t.Column<Guid>(nullable: true),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Open"),
                Type = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Sale"),
                OriginalTransactionId = t.Column<Guid>(nullable: true),
                SubTotal = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                TaxTotal = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                DiscountTotal = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                Total = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                AmountPaid = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                ChangeDue = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                Notes = t.Column<string>(maxLength: 1000, nullable: true),
                CompletedAt = t.Column<DateTime>(nullable: true),
                VoidedAt = t.Column<DateTime>(nullable: true),
                VoidedBy = t.Column<Guid>(nullable: true),
                VoidReason = t.Column<string>(maxLength: 500, nullable: true),
                IsSynced = t.Column<bool>(nullable: false, defaultValue: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t => t.PrimaryKey("PK_Transactions", x => x.Id));

        migrationBuilder.CreateIndex(name: "IX_Transactions_TransactionNumber",
            table: "Transactions", column: "TransactionNumber", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Transactions_TenantId_StoreId_CreatedAt",
            table: "Transactions", columns: ["TenantId", "StoreId", "CreatedAt"]);
        migrationBuilder.CreateIndex(name: "IX_Transactions_TerminalId",
            table: "Transactions", column: "TerminalId");
        migrationBuilder.CreateIndex(name: "IX_Transactions_CashierId",
            table: "Transactions", column: "CashierId");

        // ── LineItems ──────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "LineItems",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TransactionId = t.Column<Guid>(nullable: false),
                ProductId = t.Column<Guid>(nullable: false),
                ProductName = t.Column<string>(maxLength: 200, nullable: false),
                ProductSku = t.Column<string>(maxLength: 50, nullable: false),
                Quantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                UnitPrice = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                TaxRate = t.Column<decimal>(precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                TaxAmount = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                DiscountAmount = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                LineTotal = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                IsPriceOverridden = t.Column<bool>(nullable: false, defaultValue: false),
                PriceOverrideApprovedBy = t.Column<Guid>(nullable: true),
                IsVoided = t.Column<bool>(nullable: false, defaultValue: false),
                VoidedBy = t.Column<Guid>(nullable: true),
                VoidedAt = t.Column<DateTime>(nullable: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_LineItems", x => x.Id);
                t.ForeignKey("FK_LineItems_Transactions", x => x.TransactionId,
                    principalTable: "Transactions", principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_LineItems_TransactionId",
            table: "LineItems", column: "TransactionId");

        // ── Payments ───────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Payments",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                TransactionId = t.Column<Guid>(nullable: false),
                Method = t.Column<string>(maxLength: 20, nullable: false),
                Amount = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Pending"),
                GatewayReference = t.Column<string>(maxLength: 200, nullable: true),
                GatewayResponse = t.Column<string>(maxLength: 1000, nullable: true),
                MobileNumber = t.Column<string>(maxLength: 20, nullable: true),
                ProcessedAt = t.Column<DateTime>(nullable: true),
                DeclineReason = t.Column<string>(maxLength: 500, nullable: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Payments", x => x.Id);
                t.ForeignKey("FK_Payments_Transactions", x => x.TransactionId,
                    principalTable: "Transactions", principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Payments_TransactionId",
            table: "Payments", column: "TransactionId");

        // ── CreditAccounts ─────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "CreditAccounts",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                CustomerId = t.Column<Guid>(nullable: false),
                StoreId = t.Column<Guid>(nullable: false),
                CreditLimit = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                OutstandingBalance = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                LastPaymentAt = t.Column<DateTime>(nullable: true),
                IsActive = t.Column<bool>(nullable: false, defaultValue: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_CreditAccounts", x => x.Id);
                t.ForeignKey("FK_CreditAccounts_Customers", x => x.CustomerId,
                    principalTable: "Customers", principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_CreditAccounts_TenantId_CustomerId_StoreId",
            table: "CreditAccounts", columns: ["TenantId", "CustomerId", "StoreId"], unique: true);

        // ── CreditTransactions ─────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "CreditTransactions",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                CreditAccountId = t.Column<Guid>(nullable: false),
                TransactionId = t.Column<Guid>(nullable: true),
                Amount = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
                IsCharge = t.Column<bool>(nullable: false),
                Description = t.Column<string>(maxLength: 500, nullable: false),
                Reference = t.Column<string>(maxLength: 200, nullable: true),
                IsDeleted = t.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true),
                CreatedBy = t.Column<Guid>(nullable: true),
                UpdatedBy = t.Column<Guid>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_CreditTransactions", x => x.Id);
                t.ForeignKey("FK_CreditTransactions_CreditAccounts", x => x.CreditAccountId,
                    principalTable: "CreditAccounts", principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_CreditTransactions_CreditAccountId",
            table: "CreditTransactions", column: "CreditAccountId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("CreditTransactions");
        migrationBuilder.DropTable("CreditAccounts");
        migrationBuilder.DropTable("Payments");
        migrationBuilder.DropTable("LineItems");
        migrationBuilder.DropTable("Transactions");
        migrationBuilder.DropTable("StockLevels");
        migrationBuilder.DropTable("Products");
        migrationBuilder.DropTable("Categories");
        migrationBuilder.DropTable("VatRates");
        migrationBuilder.DropTable("Customers");
        migrationBuilder.DropTable("UserRefreshTokens");
        migrationBuilder.DropTable("Users");
        migrationBuilder.DropTable("Terminals");
        migrationBuilder.DropTable("Stores");
        migrationBuilder.DropTable("Tenants");
    }
}
