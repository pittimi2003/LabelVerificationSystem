using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerificationSystem.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260426120000_AddLabelTypesModule")]
public partial class AddLabelTypesModule : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LabelTypes",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                Columns = table.Column<string>(type: "TEXT", nullable: false),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                CreatedByUserName = table.Column<string>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                UpdatedByUserName = table.Column<string>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LabelTypes", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LabelTypes_Name",
            table: "LabelTypes",
            column: "Name",
            unique: true);

        migrationBuilder.AddColumn<string>(
            name: "LabelTypeId",
            table: "Parts",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "LabelTypeName",
            table: "Parts",
            type: "TEXT",
            nullable: false,
            defaultValue: "Por asignar");

        migrationBuilder.CreateIndex(
            name: "IX_Parts_LabelTypeId",
            table: "Parts",
            column: "LabelTypeId");

        migrationBuilder.Sql("""
INSERT INTO LabelTypes (Id, Name, Columns, IsActive, CreatedAtUtc, CreatedByUserId, CreatedByUserName, UpdatedAtUtc, UpdatedByUserId, UpdatedByUserName)
SELECT '11111111-1111-1111-1111-111111111111', 'Por asignar', '', 1, CURRENT_TIMESTAMP, 'system', 'System', CURRENT_TIMESTAMP, 'system', 'System'
WHERE NOT EXISTS (SELECT 1 FROM LabelTypes WHERE Name = 'Por asignar');

UPDATE Parts
SET LabelTypeId = '11111111-1111-1111-1111-111111111111',
    LabelTypeName = 'Por asignar'
WHERE LabelTypeId IS NULL;

INSERT INTO ModuleCatalog (Id, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc)
SELECT '91f73d66-1f6f-4ad1-af7c-7e2a80b3b7de', 'LabelTypes', 'Label Types', NULL, (SELECT COALESCE(MAX(DisplayOrder), 0) + 1 FROM ModuleCatalog), 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM ModuleCatalog WHERE Code = 'LabelTypes');

INSERT INTO ModuleActionCatalog (Id, ModuleId, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), mc.Id, action.Code, action.Name, NULL, action.DisplayOrder, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM ModuleCatalog mc
INNER JOIN (
    SELECT 'View' AS Code, 'View' AS Name, 1 AS DisplayOrder
    UNION ALL SELECT 'Create', 'Create', 2
    UNION ALL SELECT 'Edit', 'Edit', 3
    UNION ALL SELECT 'ActivateDeactivate', 'Activate/Deactivate', 4
) action ON 1 = 1
WHERE mc.Code = 'LabelTypes'
  AND NOT EXISTS (
      SELECT 1 FROM ModuleActionCatalog mac
      WHERE mac.ModuleId = mc.Id AND mac.Code = action.Code);

INSERT INTO RoleModuleAuthorization (Id, RoleId, ModuleId, Authorized, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), rc.Id, mc.Id,
    CASE
        WHEN rc.Code IN ('SuperAdmin', 'Managers') THEN 1
        ELSE 0
    END,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM RoleCatalog rc
CROSS JOIN ModuleCatalog mc
WHERE mc.Code = 'LabelTypes'
  AND NOT EXISTS (
      SELECT 1 FROM RoleModuleAuthorization rma
      WHERE rma.RoleId = rc.Id AND rma.ModuleId = mc.Id);

INSERT INTO RoleModuleActionAuthorization (Id, RoleId, ModuleActionId, Authorized, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), rc.Id, mac.Id,
    CASE
        WHEN rc.Code = 'SuperAdmin' THEN 1
        WHEN rc.Code = 'Managers' AND mac.Code = 'View' THEN 1
        ELSE 0
    END,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM RoleCatalog rc
CROSS JOIN ModuleActionCatalog mac
INNER JOIN ModuleCatalog mc ON mc.Id = mac.ModuleId
WHERE mc.Code = 'LabelTypes'
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
    SELECT mac.Id FROM ModuleActionCatalog mac
    INNER JOIN ModuleCatalog mc ON mc.Id = mac.ModuleId
    WHERE mc.Code = 'LabelTypes');

DELETE FROM RoleModuleAuthorization
WHERE ModuleId IN (SELECT Id FROM ModuleCatalog WHERE Code = 'LabelTypes');

DELETE FROM ModuleActionCatalog
WHERE ModuleId IN (SELECT Id FROM ModuleCatalog WHERE Code = 'LabelTypes');

DELETE FROM ModuleCatalog WHERE Code = 'LabelTypes';
""");

        migrationBuilder.DropIndex(name: "IX_Parts_LabelTypeId", table: "Parts");
        migrationBuilder.DropColumn(name: "LabelTypeId", table: "Parts");
        migrationBuilder.DropColumn(name: "LabelTypeName", table: "Parts");

        migrationBuilder.DropTable(name: "LabelTypes");
    }
}
