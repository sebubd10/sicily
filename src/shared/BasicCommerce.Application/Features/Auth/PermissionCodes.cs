namespace BasicCommerce.Application.Features.Auth;

/// <summary>All permission code constants. Each code maps to an ApiPermission record.</summary>
public static class PermissionCodes
{
    public static class Dashboard   { public const string View = "dashboard:view"; }

    public static class Products
    {
        public const string Read   = "products:read";
        public const string Create = "products:create";
        public const string Update = "products:update";
        public const string Delete = "products:delete";
    }

    public static class Categories  { public const string Read = "categories:read"; public const string Write = "categories:write"; }
    public static class Manufacturers { public const string Read = "manufacturers:read"; public const string Write = "manufacturers:write"; }
    public static class ProductTags { public const string Read = "product-tags:read"; public const string Write = "product-tags:write"; }

    public static class Inventory
    {
        public const string StockRead    = "inventory:stock:read";
        public const string StockAdjust  = "inventory:stock:adjust";
        public const string BatchesRead  = "inventory:batches:read";
        public const string WarehouseRead = "inventory:warehouses:read";
        public const string MovementsRead = "inventory:movements:read";
    }

    public static class Purchasing
    {
        public const string Read              = "purchasing:read";
        public const string Create            = "purchasing:create";
        public const string Receive           = "purchasing:receive";
        public const string Cancel            = "purchasing:cancel";
        public const string CatalogueRead     = "purchasing:catalogue:read";
        public const string CatalogueWrite    = "purchasing:catalogue:write";
    }

    public static class Suppliers       { public const string Read = "suppliers:read"; public const string Write = "suppliers:write"; }
    public static class SupplierReturns { public const string Read = "supplier-returns:read"; public const string Write = "supplier-returns:write"; }

    public static class Sales
    {
        public const string TransactionsRead   = "sales:transactions:read";
        public const string TransactionsCreate = "sales:transactions:create";
        public const string TransactionsVoid   = "sales:transactions:void";
    }

    public static class Customers
    {
        public const string Read         = "customers:read";
        public const string Write        = "customers:write";
        public const string CreditRead   = "customers:credit:read";
        public const string CreditAdjust = "customers:credit:adjust";
    }

    public static class Promotions  { public const string Read = "promotions:read"; public const string Write = "promotions:write"; }
    public static class GiftCards   { public const string Read = "gift-cards:read"; public const string Issue = "gift-cards:issue"; public const string Activate = "gift-cards:activate"; }
    public static class RewardPoints { public const string Read = "rewards:read"; public const string Adjust = "rewards:adjust"; }

    public static class Labels
    {
        public const string TemplatesRead  = "labels:templates:read";
        public const string TemplatesWrite = "labels:templates:write";
        public const string JobsCreate     = "labels:jobs:create";
        public const string JobsRender     = "labels:jobs:render";
    }

    public static class Till
    {
        public const string Open       = "till:open";
        public const string Close      = "till:close";
        public const string Read       = "till:read";
        public const string PettyCash  = "till:pettycash";
    }

    public static class Reports
    {
        public const string DailySales = "reports:daily";
        public const string StockReport = "reports:stock";
        public const string ZReport    = "reports:z";
    }

    public static class Users     { public const string Read = "users:read"; public const string Create = "users:create"; public const string Manage = "users:manage"; }
    public static class UserTypes { public const string Read = "user-types:read"; public const string Write = "user-types:write"; }
    public static class Menus     { public const string Read = "menus:read"; }
    public static class Permissions { public const string Read = "permissions:read"; }

    /// <summary>Returns every permission code defined in this class.</summary>
    public static IReadOnlyList<(string Code, string Name, string Group)> All =>
    [
        (Dashboard.View,                 "View Dashboard",               "Dashboard"),

        (Products.Read,                  "View Products",                "Products"),
        (Products.Create,                "Create Products",              "Products"),
        (Products.Update,                "Edit Products",                "Products"),
        (Products.Delete,                "Delete Products",              "Products"),

        (Categories.Read,                "View Categories",              "Products"),
        (Categories.Write,               "Manage Categories",            "Products"),
        (Manufacturers.Read,             "View Manufacturers",           "Products"),
        (Manufacturers.Write,            "Manage Manufacturers",         "Products"),
        (ProductTags.Read,               "View Product Tags",            "Products"),
        (ProductTags.Write,              "Manage Product Tags",          "Products"),

        (Inventory.StockRead,            "View Stock Levels",            "Inventory"),
        (Inventory.StockAdjust,          "Adjust Stock",                 "Inventory"),
        (Inventory.BatchesRead,          "View Stock Batches",           "Inventory"),
        (Inventory.WarehouseRead,        "View Warehouses",              "Inventory"),
        (Inventory.MovementsRead,        "View Stock Movements",         "Inventory"),

        (Purchasing.Read,                "View Purchase Orders",         "Purchasing"),
        (Purchasing.Create,              "Create Purchase Orders",       "Purchasing"),
        (Purchasing.Receive,             "Receive Purchase Orders",      "Purchasing"),
        (Purchasing.Cancel,              "Cancel Purchase Orders",       "Purchasing"),
        (Purchasing.CatalogueRead,       "View Supplier Catalogue",      "Purchasing"),
        (Purchasing.CatalogueWrite,      "Manage Supplier Catalogue",    "Purchasing"),
        (Suppliers.Read,                 "View Suppliers",               "Purchasing"),
        (Suppliers.Write,                "Manage Suppliers",             "Purchasing"),
        (SupplierReturns.Read,           "View Supplier Returns",        "Purchasing"),
        (SupplierReturns.Write,          "Create Supplier Returns",      "Purchasing"),

        (Sales.TransactionsRead,         "View Transactions",            "Sales"),
        (Sales.TransactionsCreate,       "Create Transactions",          "Sales"),
        (Sales.TransactionsVoid,         "Void Transactions",            "Sales"),
        (Customers.Read,                 "View Customers",               "Sales"),
        (Customers.Write,                "Manage Customers",             "Sales"),
        (Customers.CreditRead,           "View Credit Accounts",         "Sales"),
        (Customers.CreditAdjust,         "Adjust Credit Accounts",       "Sales"),

        (Promotions.Read,                "View Promotions",              "Promotions"),
        (Promotions.Write,               "Manage Promotions",            "Promotions"),
        (GiftCards.Read,                 "View Gift Cards",              "Promotions"),
        (GiftCards.Issue,                "Issue Gift Cards",             "Promotions"),
        (GiftCards.Activate,             "Activate Gift Cards",          "Promotions"),
        (RewardPoints.Read,              "View Reward Points",           "Promotions"),
        (RewardPoints.Adjust,            "Adjust Reward Points",         "Promotions"),

        (Labels.TemplatesRead,           "View Label Templates",         "Labels"),
        (Labels.TemplatesWrite,          "Manage Label Templates",       "Labels"),
        (Labels.JobsCreate,              "Create Print Jobs",            "Labels"),
        (Labels.JobsRender,              "Render & Print Labels",        "Labels"),

        (Till.Open,                      "Open Till Session",            "Till"),
        (Till.Close,                     "Close Till Session",           "Till"),
        (Till.Read,                      "View Till Sessions",           "Till"),
        (Till.PettyCash,                 "Record Petty Cash",            "Till"),

        (Reports.DailySales,             "Daily Sales Report",           "Reports"),
        (Reports.StockReport,            "Stock Report",                 "Reports"),
        (Reports.ZReport,                "Z-Report",                     "Reports"),

        (Users.Read,                     "View Users",                   "Settings"),
        (Users.Create,                   "Create Users",                 "Settings"),
        (Users.Manage,                   "Manage Users",                 "Settings"),
        (UserTypes.Read,                 "View User Types",              "Settings"),
        (UserTypes.Write,                "Manage User Types",            "Settings"),
        (Menus.Read,                     "View Menus",                   "Settings"),
        (Permissions.Read,               "View Permissions",             "Settings"),
    ];
}
