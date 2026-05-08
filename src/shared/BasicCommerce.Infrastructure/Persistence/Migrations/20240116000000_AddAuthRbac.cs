using Microsoft.EntityFrameworkCore.Migrations;

namespace BasicCommerce.Infrastructure.Persistence.Migrations;

public partial class AddAuthRbac : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── ApiPermissions ────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "ApiPermissions",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                Code = t.Column<string>(maxLength: 100, nullable: false),
                Name = t.Column<string>(maxLength: 200, nullable: false),
                Group = t.Column<string>(maxLength: 100, nullable: false),
                Description = t.Column<string>(maxLength: 500, nullable: true),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t => t.PrimaryKey("PK_ApiPermissions", x => x.Id));

        migrationBuilder.CreateIndex("IX_ApiPermissions_Code", "ApiPermissions", "Code", unique: true);
        migrationBuilder.CreateIndex("IX_ApiPermissions_Group", "ApiPermissions", "Group");

        // ── AppMenus ──────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "AppMenus",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 100, nullable: false),
                Icon = t.Column<string>(maxLength: 50, nullable: true),
                SortOrder = t.Column<int>(nullable: false, defaultValue: 0),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t => t.PrimaryKey("PK_AppMenus", x => x.Id));

        // ── AppSubMenus ───────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "AppSubMenus",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                MenuId = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 100, nullable: false),
                Icon = t.Column<string>(maxLength: 50, nullable: true),
                Route = t.Column<string>(maxLength: 200, nullable: false),
                PermissionCode = t.Column<string>(maxLength: 100, nullable: true),
                SortOrder = t.Column<int>(nullable: false, defaultValue: 0),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_AppSubMenus", x => x.Id);
                t.ForeignKey("FK_AppSubMenus_AppMenus_MenuId", x => x.MenuId,
                    "AppMenus", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_AppSubMenus_MenuId", "AppSubMenus", "MenuId");

        // ── UserTypes ─────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "UserTypes",
            columns: t => new
            {
                Id = t.Column<Guid>(nullable: false),
                TenantId = t.Column<Guid>(nullable: false),
                Name = t.Column<string>(maxLength: 100, nullable: false),
                Description = t.Column<string>(maxLength: 500, nullable: true),
                IsSystem = t.Column<bool>(nullable: false, defaultValue: false),
                SortOrder = t.Column<int>(nullable: false, defaultValue: 0),
                Color = t.Column<string>(maxLength: 20, nullable: true),
                Status = t.Column<string>(maxLength: 20, nullable: false, defaultValue: "Active"),
                CreatedAt = t.Column<DateTime>(nullable: false),
                UpdatedAt = t.Column<DateTime>(nullable: true)
            },
            constraints: t => t.PrimaryKey("PK_UserTypes", x => x.Id));

        migrationBuilder.CreateIndex("IX_UserTypes_TenantId_Name", "UserTypes",
            ["TenantId", "Name"], unique: true);

        // ── UserTypeSubMenuAccess junction ────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "UserTypeSubMenuAccess",
            columns: t => new
            {
                UserTypeId = t.Column<Guid>(nullable: false),
                SubMenuId = t.Column<Guid>(nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_UserTypeSubMenuAccess", x => new { x.UserTypeId, x.SubMenuId });
                t.ForeignKey("FK_UTMA_UserType", x => x.UserTypeId, "UserTypes", "Id",
                    onDelete: ReferentialAction.Cascade);
                t.ForeignKey("FK_UTMA_SubMenu", x => x.SubMenuId, "AppSubMenus", "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // ── UserTypeApiPermission junction ────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "UserTypeApiPermission",
            columns: t => new
            {
                UserTypeId = t.Column<Guid>(nullable: false),
                ApiPermissionId = t.Column<Guid>(nullable: false)
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_UserTypeApiPermission", x => new { x.UserTypeId, x.ApiPermissionId });
                t.ForeignKey("FK_UTAP_UserType", x => x.UserTypeId, "UserTypes", "Id",
                    onDelete: ReferentialAction.Cascade);
                t.ForeignKey("FK_UTAP_Permission", x => x.ApiPermissionId, "ApiPermissions", "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // ── Add UserTypeId to Users ───────────────────────────────────────────
        migrationBuilder.AddColumn<Guid>(
            name: "UserTypeId",
            table: "Users",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_UserTypeId",
            table: "Users",
            column: "UserTypeId",
            filter: "\"UserTypeId\" IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("UserTypeId", "Users");
        migrationBuilder.DropTable("UserTypeApiPermission");
        migrationBuilder.DropTable("UserTypeSubMenuAccess");
        migrationBuilder.DropTable("UserTypes");
        migrationBuilder.DropTable("AppSubMenus");
        migrationBuilder.DropTable("AppMenus");
        migrationBuilder.DropTable("ApiPermissions");
    }
}
