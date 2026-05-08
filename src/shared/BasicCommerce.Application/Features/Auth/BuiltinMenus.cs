namespace BasicCommerce.Application.Features.Auth;

/// <summary>Seed definitions for all admin dashboard menus and sub-menus.</summary>
public static class BuiltinMenus
{
    public record MenuDef(string Name, string Icon, int Sort, SubMenuDef[] Items);
    public record SubMenuDef(string Name, string Route, string? Permission, string? Icon, int Sort);

    public static readonly IReadOnlyList<MenuDef> All =
    [
        new("Dashboard", "grid", 1,
        [
            new("Overview",             "/dashboard",                   PermissionCodes.Dashboard.View,             null, 1),
        ]),

        new("Products", "box", 2,
        [
            new("Product List",         "/products",                    PermissionCodes.Products.Read,              null, 1),
            new("Categories",           "/categories",                  PermissionCodes.Categories.Read,            null, 2),
            new("Manufacturers",        "/manufacturers",               PermissionCodes.Manufacturers.Read,         null, 3),
            new("Product Tags",         "/product-tags",                PermissionCodes.ProductTags.Read,           null, 4),
            new("Product Reviews",      "/product-reviews",             PermissionCodes.Products.Read,              null, 5),
        ]),

        new("Inventory", "warehouse", 3,
        [
            new("Stock Levels",         "/inventory/stock",             PermissionCodes.Inventory.StockRead,        null, 1),
            new("Stock Batches",        "/inventory/batches",           PermissionCodes.Inventory.BatchesRead,      null, 2),
            new("Warehouse Stock",      "/inventory/warehouses",        PermissionCodes.Inventory.WarehouseRead,    null, 3),
            new("Stock Movements",      "/inventory/movements",         PermissionCodes.Inventory.MovementsRead,    null, 4),
        ]),

        new("Purchasing", "truck", 4,
        [
            new("Purchase Orders",      "/purchasing/orders",           PermissionCodes.Purchasing.Read,            null, 1),
            new("Suppliers",            "/purchasing/suppliers",        PermissionCodes.Suppliers.Read,             null, 2),
            new("Supplier Catalogue",   "/purchasing/catalogue",        PermissionCodes.Purchasing.CatalogueRead,   null, 3),
            new("Supplier Returns",     "/purchasing/returns",          PermissionCodes.SupplierReturns.Read,       null, 4),
        ]),

        new("Sales", "receipt", 5,
        [
            new("Transactions",         "/sales/transactions",          PermissionCodes.Sales.TransactionsRead,     null, 1),
            new("Customers",            "/sales/customers",             PermissionCodes.Customers.Read,             null, 2),
            new("Credit Accounts",      "/sales/credit",                PermissionCodes.Customers.CreditRead,       null, 3),
        ]),

        new("Promotions", "tag", 6,
        [
            new("Promotions",           "/promotions",                  PermissionCodes.Promotions.Read,            null, 1),
            new("Gift Cards",           "/promotions/gift-cards",       PermissionCodes.GiftCards.Read,             null, 2),
            new("Reward Points",        "/promotions/rewards",          PermissionCodes.RewardPoints.Read,          null, 3),
        ]),

        new("Labels", "printer", 7,
        [
            new("Label Templates",      "/labels/templates",            PermissionCodes.Labels.TemplatesRead,       null, 1),
            new("Print Jobs",           "/labels/jobs",                 PermissionCodes.Labels.JobsCreate,          null, 2),
        ]),

        new("Till Management", "cash-register", 8,
        [
            new("Sessions",             "/till/sessions",               PermissionCodes.Till.Read,                  null, 1),
            new("Petty Cash",           "/till/petty-cash",             PermissionCodes.Till.PettyCash,             null, 2),
        ]),

        new("Reports", "chart-bar", 9,
        [
            new("Daily Sales",          "/reports/daily",               PermissionCodes.Reports.DailySales,         null, 1),
            new("Stock Report",         "/reports/stock",               PermissionCodes.Reports.StockReport,        null, 2),
            new("Z-Report",             "/reports/z-report",            PermissionCodes.Reports.ZReport,            null, 3),
        ]),

        new("Settings", "cog", 10,
        [
            new("Users",                "/settings/users",              PermissionCodes.Users.Read,                 null, 1),
            new("User Types",           "/settings/user-types",         PermissionCodes.UserTypes.Read,             null, 2),
            new("Stores",               "/settings/stores",             null,                                       null, 3),
            new("Terminals",            "/settings/terminals",          null,                                       null, 4),
            new("VAT Rates",            "/settings/vat-rates",          null,                                       null, 5),
        ]),
    ];
}
