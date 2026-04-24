using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerificationSystem.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260424103000_AddRolesCatalogAuthorizationModule")]
public partial class AddRolesCatalogAuthorizationModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
INSERT INTO ModuleCatalog (Id, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc)
SELECT '81f73d66-1f6f-4ad1-af7c-7e2a80b3b7de', 'RolesCatalog', 'Roles Catalog', NULL, (SELECT COALESCE(MAX(DisplayOrder), 0) + 1 FROM ModuleCatalog), 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM ModuleCatalog WHERE Code = 'RolesCatalog');

INSERT INTO ModuleActionCatalog (Id, ModuleId, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), mc.Id, action.Code, action.Name, NULL, action.DisplayOrder, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM ModuleCatalog mc
INNER JOIN (
    SELECT 'View' AS Code, 'View' AS Name, 1 AS DisplayOrder
    UNION ALL SELECT 'Create', 'Create', 2
    UNION ALL SELECT 'Edit', 'Edit', 3
    UNION ALL SELECT 'ActivateDeactivate', 'Activate/Deactivate', 4
) action ON 1 = 1
WHERE mc.Code = 'RolesCatalog'
  AND NOT EXISTS (
      SELECT 1 FROM ModuleActionCatalog mac
      WHERE mac.ModuleId = mc.Id AND mac.Code = action.Code);

INSERT INTO RoleModuleAuthorization (Id, RoleId, ModuleId, Authorized, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), rc.Id, mc.Id,
    CASE
        WHEN rc.Code = 'SuperAdmin' THEN 1
        ELSE 0
    END,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM RoleCatalog rc
CROSS JOIN ModuleCatalog mc
WHERE mc.Code = 'RolesCatalog'
  AND NOT EXISTS (
      SELECT 1 FROM RoleModuleAuthorization rma
      WHERE rma.RoleId = rc.Id AND rma.ModuleId = mc.Id);

INSERT INTO RoleModuleActionAuthorization (Id, RoleId, ModuleActionId, Authorized, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), rc.Id, mac.Id,
    CASE
        WHEN rc.Code = 'SuperAdmin' THEN 1
        ELSE 0
    END,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM RoleCatalog rc
CROSS JOIN ModuleActionCatalog mac
INNER JOIN ModuleCatalog mc ON mc.Id = mac.ModuleId
WHERE mc.Code = 'RolesCatalog'
  AND NOT EXISTS (
      SELECT 1 FROM RoleModuleActionAuthorization rmaa
      WHERE rmaa.RoleId = rc.Id AND rmaa.ModuleActionId = mac.Id);

UPDATE RoleModuleAuthorization
SET Authorized = 1,
    UpdatedAtUtc = CURRENT_TIMESTAMP
WHERE RoleId IN (SELECT Id FROM RoleCatalog WHERE Code = 'SuperAdmin')
  AND ModuleId IN (
      SELECT Id FROM ModuleCatalog
      WHERE Code IN ('PartsCatalog', 'RolesCatalog'));

UPDATE RoleModuleActionAuthorization
SET Authorized = 1,
    UpdatedAtUtc = CURRENT_TIMESTAMP
WHERE RoleId IN (SELECT Id FROM RoleCatalog WHERE Code = 'SuperAdmin')
  AND ModuleActionId IN (
      SELECT mac.Id
      FROM ModuleActionCatalog mac
      INNER JOIN ModuleCatalog mc ON mc.Id = mac.ModuleId
      WHERE (mc.Code = 'PartsCatalog' AND mac.Code IN ('View', 'Create', 'Edit'))
         OR (mc.Code = 'RolesCatalog' AND mac.Code IN ('View', 'Create', 'Edit', 'ActivateDeactivate')));
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DELETE FROM RoleModuleActionAuthorization
WHERE ModuleActionId IN (
    SELECT mac.Id
    FROM ModuleActionCatalog mac
    INNER JOIN ModuleCatalog mc ON mc.Id = mac.ModuleId
    WHERE mc.Code = 'RolesCatalog');

DELETE FROM RoleModuleAuthorization
WHERE ModuleId IN (
    SELECT Id FROM ModuleCatalog WHERE Code = 'RolesCatalog');

DELETE FROM ModuleActionCatalog
WHERE ModuleId IN (
    SELECT Id FROM ModuleCatalog WHERE Code = 'RolesCatalog');

DELETE FROM ModuleCatalog
WHERE Code = 'RolesCatalog';
""");
    }
}
