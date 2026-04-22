using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerificationSystem.Infrastructure.Persistence.Migrations
{
    public partial class AddAuthorizationMatrixAdministrationModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
INSERT INTO ModuleCatalog (Id, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc)
SELECT 'c377b03e-341a-4f06-8f0a-af306d3650e1', 'AuthorizationMatrixAdministration', 'Authorization Matrix Administration', NULL, 8, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM ModuleCatalog WHERE Code = 'AuthorizationMatrixAdministration');

INSERT INTO ModuleActionCatalog (Id, ModuleId, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc)
SELECT '85cf2426-c847-4e7b-8f54-e09030fe988e', mc.Id, 'Manage', 'Manage', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM ModuleCatalog mc
WHERE mc.Code = 'AuthorizationMatrixAdministration'
  AND NOT EXISTS (
      SELECT 1 FROM ModuleActionCatalog mac
      WHERE mac.ModuleId = mc.Id AND mac.Code = 'Manage');

INSERT INTO RoleModuleAuthorization (Id, RoleId, ModuleId, Authorized, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), rc.Id, mc.Id,
    CASE
        WHEN rc.Code = 'SuperAdmin' THEN 1
        ELSE 0
    END,
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM RoleCatalog rc
CROSS JOIN ModuleCatalog mc
WHERE mc.Code = 'AuthorizationMatrixAdministration'
  AND NOT EXISTS (
      SELECT 1 FROM RoleModuleAuthorization rma
      WHERE rma.RoleId = rc.Id AND rma.ModuleId = mc.Id);

INSERT INTO RoleModuleActionAuthorization (Id, RoleId, ModuleActionId, Authorized, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), rc.Id, mac.Id,
    CASE
        WHEN rc.Code = 'SuperAdmin' THEN 1
        ELSE 0
    END,
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM RoleCatalog rc
CROSS JOIN ModuleActionCatalog mac
INNER JOIN ModuleCatalog mc ON mc.Id = mac.ModuleId
WHERE mc.Code = 'AuthorizationMatrixAdministration'
  AND mac.Code = 'Manage'
  AND NOT EXISTS (
      SELECT 1 FROM RoleModuleActionAuthorization rmaa
      WHERE rmaa.RoleId = rc.Id AND rmaa.ModuleActionId = mac.Id);
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
    WHERE mc.Code = 'AuthorizationMatrixAdministration' AND mac.Code = 'Manage');

DELETE FROM RoleModuleAuthorization
WHERE ModuleId IN (
    SELECT Id FROM ModuleCatalog WHERE Code = 'AuthorizationMatrixAdministration');

DELETE FROM ModuleActionCatalog
WHERE ModuleId IN (
    SELECT Id FROM ModuleCatalog WHERE Code = 'AuthorizationMatrixAdministration');

DELETE FROM ModuleCatalog
WHERE Code = 'AuthorizationMatrixAdministration';
""");
        }
    }
}
