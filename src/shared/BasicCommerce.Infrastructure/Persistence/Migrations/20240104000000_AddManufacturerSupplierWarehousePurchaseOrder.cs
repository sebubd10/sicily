using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddManufacturerSupplierWarehousePurchaseOrder : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Manufacturers
        migrationBuilder.CreateTable("Manufacturers", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            Name = t.Column<string>(maxLength: 200, nullable: false),
            Code = t.Column<string>(maxLength: 50, nullable: true),
            Country = t.Column<string>(maxLength: 100, nullable: true),
            Website = t.Column<string>(maxLength: 500, nullable: true),
            ContactEmail = t.Column<string>(maxLength: 200, nullable: true),
            Notes = t.Column<string>(maxLength: 1000, nullable: true),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t => t.PrimaryKey("PK_Manufacturers", x => x.Id));

        migrationBuilder.CreateIndex("IX_Manufacturers_TenantId_Name", "Manufacturers", ["TenantId", "Name"]);
        migrationBuilder.CreateIndex("IX_Manufacturers_TenantId_Code", "Manufacturers", ["TenantId", "Code"]);

        // Suppliers
        migrationBuilder.CreateTable("Suppliers", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            Name = t.Column<string>(maxLength: 200, nullable: false),
            Code = t.Column<string>(maxLength: 50, nullable: false),
            ContactName = t.Column<string>(maxLength: 200, nullable: true),
            Email = t.Column<string>(maxLength: 200, nullable: true),
            Phone = t.Column<string>(maxLength: 50, nullable: true),
            AddressLine1 = t.Column<string>(maxLength: 200, nullable: true),
            AddressLine2 = t.Column<string>(maxLength: 200, nullable: true),
            AddressCity = t.Column<string>(maxLength: 100, nullable: true),
            AddressDistrict = t.Column<string>(maxLength: 100, nullable: true),
            AddressPostalCode = t.Column<string>(maxLength: 20, nullable: true),
            AddressCountry = t.Column<string>(maxLength: 100, nullable: true),
            LeadTimeDays = t.Column<int>(nullable: false, defaultValue: 0),
            Notes = t.Column<string>(maxLength: 1000, nullable: true),
            ManufacturerId = t.Column<Guid>(nullable: true),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_Suppliers", x => x.Id);
            t.ForeignKey("FK_Suppliers_Manufacturers_ManufacturerId", x => x.ManufacturerId,
                "Manufacturers", "Id", onDelete: ReferentialAction.SetNull);
        });

        migrationBuilder.CreateIndex("IX_Suppliers_TenantId_Code", "Suppliers", ["TenantId", "Code"], unique: true);
        migrationBuilder.CreateIndex("IX_Suppliers_ManufacturerId", "Suppliers", "ManufacturerId");

        // Warehouses
        migrationBuilder.CreateTable("Warehouses", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            Name = t.Column<string>(maxLength: 200, nullable: false),
            Code = t.Column<string>(maxLength: 50, nullable: false),
            Phone = t.Column<string>(maxLength: 50, nullable: true),
            Email = t.Column<string>(maxLength: 200, nullable: true),
            AddressLine1 = t.Column<string>(maxLength: 200, nullable: false),
            AddressLine2 = t.Column<string>(maxLength: 200, nullable: true),
            AddressCity = t.Column<string>(maxLength: 100, nullable: false),
            AddressDistrict = t.Column<string>(maxLength: 100, nullable: false),
            AddressPostalCode = t.Column<string>(maxLength: 20, nullable: false),
            AddressCountry = t.Column<string>(maxLength: 100, nullable: false),
            IsDefault = t.Column<bool>(nullable: false, defaultValue: false),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t => t.PrimaryKey("PK_Warehouses", x => x.Id));

        migrationBuilder.CreateIndex("IX_Warehouses_TenantId_Code", "Warehouses", ["TenantId", "Code"], unique: true);

        // WarehouseStockLevels
        migrationBuilder.CreateTable("WarehouseStockLevels", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            WarehouseId = t.Column<Guid>(nullable: false),
            ProductId = t.Column<Guid>(nullable: false),
            Quantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
            ReservedQuantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
            LowStockThreshold = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 10m),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_WarehouseStockLevels", x => x.Id);
            t.ForeignKey("FK_WarehouseStockLevels_Warehouses_WarehouseId", x => x.WarehouseId,
                "Warehouses", "Id", onDelete: ReferentialAction.Restrict);
            t.ForeignKey("FK_WarehouseStockLevels_Products_ProductId", x => x.ProductId,
                "Products", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateIndex("IX_WarehouseStockLevels_TenantId_WarehouseId_ProductId",
            "WarehouseStockLevels", ["TenantId", "WarehouseId", "ProductId"], unique: true);

        // WarehouseMovements
        migrationBuilder.CreateTable("WarehouseMovements", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            WarehouseId = t.Column<Guid>(nullable: false),
            ProductId = t.Column<Guid>(nullable: false),
            MovementType = t.Column<string>(maxLength: 30, nullable: false),
            Quantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
            QuantityBefore = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
            QuantityAfter = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
            RelatedStoreId = t.Column<Guid>(nullable: true),
            PurchaseOrderId = t.Column<Guid>(nullable: true),
            Reference = t.Column<string>(maxLength: 100, nullable: true),
            Notes = t.Column<string>(maxLength: 500, nullable: true),
            RecordedByUserId = t.Column<Guid>(nullable: false),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_WarehouseMovements", x => x.Id);
            t.ForeignKey("FK_WarehouseMovements_Warehouses_WarehouseId", x => x.WarehouseId,
                "Warehouses", "Id", onDelete: ReferentialAction.Restrict);
            t.ForeignKey("FK_WarehouseMovements_Products_ProductId", x => x.ProductId,
                "Products", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateIndex("IX_WarehouseMovements_TenantId_WarehouseId_CreatedAt",
            "WarehouseMovements", ["TenantId", "WarehouseId", "CreatedAt"]);
        migrationBuilder.CreateIndex("IX_WarehouseMovements_TenantId_WarehouseId_ProductId",
            "WarehouseMovements", ["TenantId", "WarehouseId", "ProductId"]);

        // PurchaseOrders
        migrationBuilder.CreateTable("PurchaseOrders", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            TenantId = t.Column<Guid>(nullable: false),
            OrderNumber = t.Column<string>(maxLength: 50, nullable: false),
            SupplierId = t.Column<Guid>(nullable: false),
            WarehouseId = t.Column<Guid>(nullable: false),
            PurchaseOrderStatus = t.Column<string>(maxLength: 30, nullable: false, defaultValue: "Draft"),
            OrderDate = t.Column<DateTime>(nullable: false),
            ExpectedDate = t.Column<DateTime>(nullable: true),
            ReceivedDate = t.Column<DateTime>(nullable: true),
            Notes = t.Column<string>(maxLength: 1000, nullable: true),
            Currency = t.Column<string>(maxLength: 3, nullable: false, defaultValue: "BDT"),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_PurchaseOrders", x => x.Id);
            t.ForeignKey("FK_PurchaseOrders_Suppliers_SupplierId", x => x.SupplierId,
                "Suppliers", "Id", onDelete: ReferentialAction.Restrict);
            t.ForeignKey("FK_PurchaseOrders_Warehouses_WarehouseId", x => x.WarehouseId,
                "Warehouses", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateIndex("IX_PurchaseOrders_TenantId_OrderNumber",
            "PurchaseOrders", ["TenantId", "OrderNumber"], unique: true);
        migrationBuilder.CreateIndex("IX_PurchaseOrders_TenantId_PurchaseOrderStatus",
            "PurchaseOrders", ["TenantId", "PurchaseOrderStatus"]);
        migrationBuilder.CreateIndex("IX_PurchaseOrders_TenantId_SupplierId",
            "PurchaseOrders", ["TenantId", "SupplierId"]);

        // PurchaseOrderItems
        migrationBuilder.CreateTable("PurchaseOrderItems", t => new
        {
            Id = t.Column<Guid>(nullable: false),
            PurchaseOrderId = t.Column<Guid>(nullable: false),
            ProductId = t.Column<Guid>(nullable: false),
            OrderedQuantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
            ReceivedQuantity = t.Column<decimal>(precision: 18, scale: 4, nullable: false, defaultValue: 0m),
            UnitCost = t.Column<decimal>(precision: 18, scale: 4, nullable: false),
            Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
            CreatedAt = t.Column<DateTime>(nullable: false),
            UpdatedAt = t.Column<DateTime>(nullable: true),
            CreatedBy = t.Column<Guid>(nullable: true),
            UpdatedBy = t.Column<Guid>(nullable: true)
        }, constraints: t =>
        {
            t.PrimaryKey("PK_PurchaseOrderItems", x => x.Id);
            t.ForeignKey("FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId", x => x.PurchaseOrderId,
                "PurchaseOrders", "Id", onDelete: ReferentialAction.Cascade);
            t.ForeignKey("FK_PurchaseOrderItems_Products_ProductId", x => x.ProductId,
                "Products", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateIndex("IX_PurchaseOrderItems_PurchaseOrderId_ProductId",
            "PurchaseOrderItems", ["PurchaseOrderId", "ProductId"], unique: true);

        // Add ManufacturerId to Products
        migrationBuilder.AddColumn<Guid>("ManufacturerId", "Products", nullable: true);
        migrationBuilder.CreateIndex("IX_Products_ManufacturerId", "Products", "ManufacturerId");
        migrationBuilder.AddForeignKey("FK_Products_Manufacturers_ManufacturerId", "Products",
            "ManufacturerId", "Manufacturers", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_Products_Manufacturers_ManufacturerId", "Products");
        migrationBuilder.DropIndex("IX_Products_ManufacturerId", "Products");
        migrationBuilder.DropColumn("ManufacturerId", "Products");

        migrationBuilder.DropTable("PurchaseOrderItems");
        migrationBuilder.DropTable("PurchaseOrders");
        migrationBuilder.DropTable("WarehouseMovements");
        migrationBuilder.DropTable("WarehouseStockLevels");
        migrationBuilder.DropTable("Warehouses");
        migrationBuilder.DropTable("Suppliers");
        migrationBuilder.DropTable("Manufacturers");
    }
}
