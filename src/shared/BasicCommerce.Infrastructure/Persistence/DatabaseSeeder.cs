using BasicCommerce.Application.Features.Auth;
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
/// </summary>
public class DatabaseSeeder
{
    private readonly BasicCommerceDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(BasicCommerceDbContext db, ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedDemoTenantAsync(ct);
        await SeedMenusAndPermissionsAsync(ct);
        _logger.LogInformation("Database seeding completed.");
    }

    private async Task SeedDemoTenantAsync(CancellationToken ct)
    {
        if (await _db.Tenants.AnyAsync(ct))
        {
            _logger.LogInformation("Seed data already present — skipping.");
            return;
        }

        _logger.LogInformation("Seeding demo tenant and reference data…");

        // ── Demo Tenant ────────────────────────────────────────────────────────
        var tenant = Tenant.Create(
            name: "BasicCommerce Demo",
            slug: "demo",
            contactEmail: "admin@basiccommerce.io",
            bin: "000000000",
            vatRegistration: "VAT-000000000");

        await _db.Tenants.AddAsync(tenant, ct);
        await _db.SaveChangesAsync(ct);

        // ── Bangladesh NBR VAT Rates ───────────────────────────────────────────
        var vatRates = new[]
        {
            VatRate.Create(tenant.Id, "Standard Rate (NBR)",    "STD",      15m, isDefault: true),
            VatRate.Create(tenant.Id, "Reduced Rate",           "RED",       5m),
            VatRate.Create(tenant.Id, "Zero Rated",             "ZERO",      0m),
            VatRate.Create(tenant.Id, "Exempt",                 "EXEMPT",    0m),
            VatRate.Create(tenant.Id, "Truncated Rate (4.5%)",  "TRUNC",   4.5m),
        };

        await _db.VatRates.AddRangeAsync(vatRates, ct);

        // ── Root Categories ────────────────────────────────────────────────────
        var categories = new[]
        {
            Category.Create(tenant.Id, "Groceries",        "মুদিখানা"),
            Category.Create(tenant.Id, "Beverages",        "পানীয়"),
            Category.Create(tenant.Id, "Dairy",            "দুগ্ধজাত পণ্য"),
            Category.Create(tenant.Id, "Bakery",           "বেকারি"),
            Category.Create(tenant.Id, "Meat & Fish",      "মাংস ও মাছ"),
            Category.Create(tenant.Id, "Fruits & Vegetables", "ফল ও সবজি"),
            Category.Create(tenant.Id, "Personal Care",    "ব্যক্তিগত পরিচর্যা"),
            Category.Create(tenant.Id, "Household",        "গৃহস্থালি"),
        };

        await _db.Categories.AddRangeAsync(categories, ct);

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
        var terminal = Terminal.Create(
            tenantId: tenant.Id,
            storeId: store.Id,
            name: "Counter 1",
            code: "T01",
            type: TerminalType.Standard);

        await _db.Terminals.AddAsync(terminal, ct);

        // ── Chain Admin User ───────────────────────────────────────────────────
        // Default password: Admin@1234 (BCrypt hashed)
        // !! Change immediately after first login !!
        const string defaultPasswordHash =
            "$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi";

        var adminUser = User.Create(
            tenantId: tenant.Id,
            employeeCode: "ADM-001",
            firstName: "System",
            lastName: "Admin",
            email: "admin@basiccommerce.io",
            passwordHash: defaultPasswordHash,
            role: UserRole.ChainAdmin);

        await _db.Users.AddAsync(adminUser, ct);

        // ── Demo Cashier ───────────────────────────────────────────────────────
        var cashierHash =
            "$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi";

        var cashier = User.Create(
            tenantId: tenant.Id,
            employeeCode: "CSH-001",
            firstName: "Demo",
            lastName: "Cashier",
            email: "cashier@basiccommerce.io",
            passwordHash: cashierHash,
            role: UserRole.Cashier,
            storeId: store.Id);

        await _db.Users.AddAsync(cashier, ct);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Seeded: Tenant={Tenant}, Store={Store}, VAT rates={VatCount}, " +
            "Categories={CatCount}, Admin={Admin}",
            tenant.Name, store.Name, vatRates.Length, categories.Length, adminUser.Email);
    }

    private async Task SeedMenusAndPermissionsAsync(CancellationToken ct)
    {
        // ── Api Permissions ────────────────────────────────────────────────────
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
            _logger.LogInformation("Seeded {Count} API permissions.", toAdd.Count);
        }

        // ── App Menus + SubMenus ───────────────────────────────────────────────
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
}
