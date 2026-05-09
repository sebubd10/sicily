using BasicCommerce.Application.Features.Auth;
using BasicCommerce.Application.Interfaces;
using BasicCommerce.Domain.Entities;
using BasicCommerce.Domain.Enums;
using BasicCommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BasicCommerce.Infrastructure.Persistence;

/// <summary>
/// Seeds required reference data on first run:
/// - Bangladesh NBR VAT rates (15% standard, 5% reduced, 0% zero-rated)
/// - A demo tenant, store, and chain admin user for development
/// - Reward points default settings
/// - App menus, sub-menus, and API permissions
/// - System user types (Chain Admin, Store Manager, Cashier) with pre-configured access
/// </summary>
public class DatabaseSeeder
{
    private readonly BasicCommerceDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(BasicCommerceDbContext db, IPasswordHasher hasher,
        ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedTenantDataAsync(ct);
        await SeedMenusAndPermissionsAsync(ct);
        await SeedSystemUserTypesAsync(ct);
        await SeedDemoUsersAsync(ct);
        _logger.LogInformation("Database seeding completed.");
    }

    // ── 1. Tenant, Store, Reference Data ──────────────────────────────────────

    private async Task SeedTenantDataAsync(CancellationToken ct)
    {
        if (await _db.Tenants.AnyAsync(ct))
        {
            _logger.LogInformation("Tenant seed data already present — skipping.");
            return;
        }

        _logger.LogInformation("Seeding demo tenant and reference data…");

        var tenant = Tenant.Create(
            name: "BasicCommerce Demo",
            slug: "demo",
            contactEmail: "admin@basiccommerce.io",
            bin: "000000000",
            vatRegistration: "VAT-000000000");

        await _db.Tenants.AddAsync(tenant, ct);
        await _db.SaveChangesAsync(ct);

        // ── VAT Rates (Bangladesh NBR) ─────────────────────────────────────────
        await _db.VatRates.AddRangeAsync(
        [
            VatRate.Create(tenant.Id, "Standard Rate (NBR)",   "STD",    15m,  isDefault: true),
            VatRate.Create(tenant.Id, "Reduced Rate",          "RED",     5m),
            VatRate.Create(tenant.Id, "Truncated Rate (4.5%)", "TRUNC",  4.5m),
            VatRate.Create(tenant.Id, "Zero Rated",            "ZERO",   0m),
            VatRate.Create(tenant.Id, "Exempt",                "EXEMPT", 0m),
        ], ct);

        // ── Root Categories ────────────────────────────────────────────────────
        await _db.Categories.AddRangeAsync(
        [
            Category.Create(tenant.Id, "Groceries",             "মুদিখানা"),
            Category.Create(tenant.Id, "Beverages",             "পানীয়"),
            Category.Create(tenant.Id, "Dairy",                 "দুগ্ধজাত পণ্য"),
            Category.Create(tenant.Id, "Bakery",                "বেকারি"),
            Category.Create(tenant.Id, "Meat & Fish",           "মাংস ও মাছ"),
            Category.Create(tenant.Id, "Fruits & Vegetables",   "ফল ও সবজি"),
            Category.Create(tenant.Id, "Personal Care",         "ব্যক্তিগত পরিচর্যা"),
            Category.Create(tenant.Id, "Household",             "গৃহস্থালি"),
        ], ct);

        // ── Demo Store ─────────────────────────────────────────────────────────
        var store = Store.Create(
            tenantId: tenant.Id,
            name: "Demo Supermarket – Dhaka",
            code: "DHAKA-01",
            address: Address.Create(
                line1: "123 Mirpur Road",
                city: "Dhaka",
                district: "Dhaka",
                postalCode: "1216",
                country: "BD"),
            phone: "+8801700000000",
            email: "dhaka01@basiccommerce.io");

        await _db.Stores.AddAsync(store, ct);

        // ── Demo Terminal ──────────────────────────────────────────────────────
        await _db.Terminals.AddAsync(Terminal.Create(
            tenantId: tenant.Id,
            storeId: store.Id,
            name: "Counter 1",
            code: "T01",
            type: TerminalType.Standard), ct);

        // ── Reward Points Settings (required for transaction features) ─────────
        await _db.RewardPointsSettings.AddAsync(
            RewardPointsSettings.CreateDefault(tenant.Id), ct);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Seeded: Tenant='{Tenant}', Store='{Store}', VAT rates=5, Categories=8.",
            tenant.Name, store.Name);
    }

    // ── 2. App Menus & API Permissions ────────────────────────────────────────

    private async Task SeedMenusAndPermissionsAsync(CancellationToken ct)
    {
        // Permissions are upserted so new codes added in code are picked up on restart.
        var existingCodes = (await _db.Set<ApiPermission>()
            .Select(p => p.Code).ToListAsync(ct)).ToHashSet();

        var toAdd = PermissionCodes.All
            .Where(p => !existingCodes.Contains(p.Code))
            .Select(p => ApiPermission.Create(p.Code, p.Name, p.Group))
            .ToList();

        if (toAdd.Count > 0)
        {
            await _db.Set<ApiPermission>().AddRangeAsync(toAdd, ct);
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Seeded {Count} new API permissions.", toAdd.Count);
        }

        if (!await _db.Set<AppMenu>().AnyAsync(ct))
        {
            foreach (var md in BuiltinMenus.All)
            {
                var menu = AppMenu.Create(md.Name, md.Icon, md.Sort);
                await _db.Set<AppMenu>().AddAsync(menu, ct);
                await _db.SaveChangesAsync(ct);

                foreach (var sd in md.Items)
                {
                    var sub = AppSubMenu.Create(menu.Id, sd.Name, sd.Route,
                        sd.Permission, sd.Icon, sd.Sort);
                    await _db.Set<AppSubMenu>().AddAsync(sub, ct);
                }
            }

            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Seeded app menus and sub-menus.");
        }
    }

    // ── 3. System User Types ──────────────────────────────────────────────────

    private async Task SeedSystemUserTypesAsync(CancellationToken ct)
    {
        if (await _db.Set<UserType>().AnyAsync(t => t.IsSystem, ct))
        {
            _logger.LogInformation("System user types already seeded — skipping.");
            return;
        }

        var tenantId = await _db.Tenants.Select(t => t.Id).FirstAsync(ct);
        var allPermissions = await _db.Set<ApiPermission>().ToListAsync(ct);
        var allSubMenus    = await _db.Set<AppSubMenu>().ToListAsync(ct);

        // ── Chain Admin: full access to everything ─────────────────────────────
        var chainAdmin = UserType.Create(tenantId, "Chain Admin",
            description: "Full system access across all stores",
            isSystem: true, sortOrder: 1, color: "#6366f1");
        chainAdmin.SetPermissions(allPermissions);
        chainAdmin.SetMenuAccess(allSubMenus);

        // ── Store Manager: all store-level operations ──────────────────────────
        var storeManagerCodes = new HashSet<string>
        {
            PermissionCodes.Dashboard.View,
            PermissionCodes.Products.Read,    PermissionCodes.Products.Create,
            PermissionCodes.Products.Update,
            PermissionCodes.Categories.Read,
            PermissionCodes.Manufacturers.Read,
            PermissionCodes.ProductTags.Read,
            PermissionCodes.Inventory.StockRead,  PermissionCodes.Inventory.StockAdjust,
            PermissionCodes.Inventory.BatchesRead, PermissionCodes.Inventory.MovementsRead,
            PermissionCodes.Purchasing.Read,   PermissionCodes.Purchasing.Create,
            PermissionCodes.Purchasing.Receive, PermissionCodes.Purchasing.Cancel,
            PermissionCodes.Purchasing.CatalogueRead,
            PermissionCodes.Suppliers.Read,
            PermissionCodes.SupplierReturns.Read, PermissionCodes.SupplierReturns.Write,
            PermissionCodes.Sales.TransactionsRead, PermissionCodes.Sales.TransactionsCreate,
            PermissionCodes.Sales.TransactionsVoid,
            PermissionCodes.Customers.Read,   PermissionCodes.Customers.Write,
            PermissionCodes.Customers.CreditRead,
            PermissionCodes.Promotions.Read,
            PermissionCodes.GiftCards.Read,   PermissionCodes.GiftCards.Issue,
            PermissionCodes.RewardPoints.Read,
            PermissionCodes.Labels.TemplatesRead, PermissionCodes.Labels.JobsCreate,
            PermissionCodes.Labels.JobsRender,
            PermissionCodes.Till.Open, PermissionCodes.Till.Close,
            PermissionCodes.Till.Read, PermissionCodes.Till.PettyCash,
            PermissionCodes.Reports.DailySales, PermissionCodes.Reports.StockReport,
            PermissionCodes.Reports.ZReport,   PermissionCodes.Reports.CategoryReport,
            PermissionCodes.Users.Read,
        };

        var storeManager = UserType.Create(tenantId, "Store Manager",
            description: "Full store-level operations and reporting",
            isSystem: true, sortOrder: 2, color: "#0ea5e9");
        storeManager.SetPermissions(
            allPermissions.Where(p => storeManagerCodes.Contains(p.Code)).ToList());
        storeManager.SetMenuAccess(
            allSubMenus.Where(s => s.PermissionCode == null
                || storeManagerCodes.Contains(s.PermissionCode)).ToList());

        // ── Cashier: POS terminal only ─────────────────────────────────────────
        var cashierCodes = new HashSet<string>
        {
            PermissionCodes.Sales.TransactionsCreate, PermissionCodes.Sales.TransactionsRead,
            PermissionCodes.Customers.Read,
            PermissionCodes.Till.Open,  PermissionCodes.Till.Close,
            PermissionCodes.Till.Read,  PermissionCodes.Till.PettyCash,
            PermissionCodes.Labels.JobsCreate, PermissionCodes.Labels.JobsRender,
            PermissionCodes.GiftCards.Read,
            PermissionCodes.RewardPoints.Read,
        };

        var cashier = UserType.Create(tenantId, "Cashier",
            description: "POS terminal access — sales, till, and customers",
            isSystem: true, sortOrder: 3, color: "#10b981");
        cashier.SetPermissions(
            allPermissions.Where(p => cashierCodes.Contains(p.Code)).ToList());
        cashier.SetMenuAccess(
            allSubMenus.Where(s => s.PermissionCode != null
                && cashierCodes.Contains(s.PermissionCode)).ToList());

        await _db.Set<UserType>().AddRangeAsync([chainAdmin, storeManager, cashier], ct);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Seeded 3 system user types: Chain Admin, Store Manager, Cashier.");
    }

    // ── 4. Demo Users ─────────────────────────────────────────────────────────

    private async Task SeedDemoUsersAsync(CancellationToken ct)
    {
        if (await _db.Users.AnyAsync(ct))
        {
            _logger.LogInformation("Users already seeded — skipping.");
            return;
        }

        var tenantId = await _db.Tenants.Select(t => t.Id).FirstAsync(ct);
        var storeId  = await _db.Stores.Select(s => s.Id).FirstAsync(ct);

        var userTypes = await _db.Set<UserType>()
            .Where(t => t.IsSystem)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);

        var chainAdminTypeId = userTypes.FirstOrDefault(t => t.Name == "Chain Admin")?.Id;
        var cashierTypeId    = userTypes.FirstOrDefault(t => t.Name == "Cashier")?.Id;

        // Default password: Admin@1234
        var defaultHash = _hasher.Hash("Admin@1234");

        var admin = User.Create(
            tenantId: tenantId,
            employeeCode: "ADM-001",
            firstName: "System",
            lastName: "Admin",
            email: "admin@basiccommerce.io",
            passwordHash: defaultHash,
            role: UserRole.ChainAdmin);
        admin.SetUserType(chainAdminTypeId);

        var cashier = User.Create(
            tenantId: tenantId,
            employeeCode: "CSH-001",
            firstName: "Demo",
            lastName: "Cashier",
            email: "cashier@basiccommerce.io",
            passwordHash: defaultHash,
            role: UserRole.Cashier,
            storeId: storeId);
        cashier.SetUserType(cashierTypeId);

        await _db.Users.AddRangeAsync([admin, cashier], ct);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Seeded users: Admin='{Admin}' (pass: Admin@1234), Cashier='{Cashier}'.",
            admin.Email, cashier.Email);
    }
}
