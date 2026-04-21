using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerificationSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRobustAuthorizationModelBlockB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleCatalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleCatalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleActionCatalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false, collation: "NOCASE"),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleActionCatalog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleActionCatalog_ModuleCatalog_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "ModuleCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleModuleAuthorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Authorized = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleModuleAuthorization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleModuleAuthorization_ModuleCatalog_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "ModuleCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleModuleAuthorization_RoleCatalog_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RoleCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemUserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SystemUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUserRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemUserRole_RoleCatalog_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RoleCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemUserRole_SystemUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "SystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemUserRole_SystemUsers_SystemUserId",
                        column: x => x.SystemUserId,
                        principalTable: "SystemUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleModuleActionAuthorization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModuleActionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Authorized = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleModuleActionAuthorization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleModuleActionAuthorization_ModuleActionCatalog_ModuleActionId",
                        column: x => x.ModuleActionId,
                        principalTable: "ModuleActionCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoleModuleActionAuthorization_RoleCatalog_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RoleCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModuleActionCatalog_ModuleId_Code",
                table: "ModuleActionCatalog",
                columns: new[] { "ModuleId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleActionCatalog_ModuleId_DisplayOrder",
                table: "ModuleActionCatalog",
                columns: new[] { "ModuleId", "DisplayOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleCatalog_Code",
                table: "ModuleCatalog",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleCatalog_DisplayOrder",
                table: "ModuleCatalog",
                column: "DisplayOrder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleCatalog_Code",
                table: "RoleCatalog",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleModuleActionAuthorization_ModuleActionId",
                table: "RoleModuleActionAuthorization",
                column: "ModuleActionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModuleActionAuthorization_RoleId_ModuleActionId",
                table: "RoleModuleActionAuthorization",
                columns: new[] { "RoleId", "ModuleActionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleModuleAuthorization_ModuleId",
                table: "RoleModuleAuthorization",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModuleAuthorization_RoleId_ModuleId",
                table: "RoleModuleAuthorization",
                columns: new[] { "RoleId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRole_AssignedByUserId",
                table: "SystemUserRole",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRole_RoleId",
                table: "SystemUserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRole_SystemUserId_IsPrimary",
                table: "SystemUserRole",
                columns: new[] { "SystemUserId", "IsPrimary" },
                unique: true,
                filter: "IsPrimary = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUserRole_SystemUserId_RoleId",
                table: "SystemUserRole",
                columns: new[] { "SystemUserId", "RoleId" },
                unique: true);

            migrationBuilder.Sql(@"""
INSERT INTO RoleCatalog (Id, Code, Name, IsActive, CreatedAtUtc, UpdatedAtUtc) VALUES
('2f60c809-f9da-4631-a3b0-bf6c11b96524', 'SuperAdmin', 'SuperAdmin', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('592767f7-b026-4cd6-b0d6-7050c302e259', 'Operators', 'Operators', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('98f4fb8e-bdb0-43f5-a6fc-72d77d3211eb', 'Managers', 'Managers', 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

INSERT INTO ModuleCatalog (Id, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc) VALUES
('2bcc6622-4b31-42f7-953f-302584fbe7b6', 'UsersAdministration', 'Users Administration', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('f2d21c54-a632-4470-a304-099ed15e5538', 'ExcelUploads', 'Excel Uploads', NULL, 2, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('2a6f3988-b8cf-40bb-a5fd-7609cb8e4b71', 'PartsCatalog', 'Parts Catalog', NULL, 3, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('f12de5db-5fce-4a79-89c2-41c63a8af4eb', 'LabelVerification', 'Label Verification', NULL, 4, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('7e9fb8f9-8770-4f05-b909-0dc4f18f9245', 'PackingLists', 'Packing Lists', NULL, 5, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('f132dccb-2bae-4c34-b28c-56e631973876', 'AuditTrail', 'Audit Trail', NULL, 6, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('b5be19b4-ff12-407a-91ee-d6f1f0f8cb20', 'SystemConfiguration', 'System Configuration', NULL, 7, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

INSERT INTO ModuleActionCatalog (Id, ModuleId, Code, Name, Description, DisplayOrder, IsActive, CreatedAtUtc, UpdatedAtUtc) VALUES
('af5e4bb7-ae4f-4d3c-a43f-4cf5a5d34b35', '2bcc6622-4b31-42f7-953f-302584fbe7b6', 'View', 'View', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('0be974ef-ef75-420f-a6cd-17ec5f4f6ed4', '2bcc6622-4b31-42f7-953f-302584fbe7b6', 'Create', 'Create', NULL, 2, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('8300715a-f9fe-4909-8617-a68d4ef0f7ba', '2bcc6622-4b31-42f7-953f-302584fbe7b6', 'Edit', 'Edit', NULL, 3, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('e0b25e73-0de4-4f6e-afdf-2bfcd84f040f', '2bcc6622-4b31-42f7-953f-302584fbe7b6', 'ActivateDeactivate', 'Activate/Deactivate', NULL, 4, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('f9359877-f2bc-4ce9-b7a0-1311d3220f24', '2bcc6622-4b31-42f7-953f-302584fbe7b6', 'ResetPassword', 'Reset Password', NULL, 5, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('f499a01f-a4fb-4cd6-a133-4234264ebf1a', 'f2d21c54-a632-4470-a304-099ed15e5538', 'View', 'View', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('d3f5768d-c7b7-4f4f-aa47-44f5827e51e7', 'f2d21c54-a632-4470-a304-099ed15e5538', 'Upload', 'Upload', NULL, 2, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('6d0367ac-c6fb-4fca-98ae-45fc53094eff', '2a6f3988-b8cf-40bb-a5fd-7609cb8e4b71', 'View', 'View', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('f76c348e-5f4c-4e66-bf26-7e0629fa6e4c', '2a6f3988-b8cf-40bb-a5fd-7609cb8e4b71', 'Create', 'Create', NULL, 2, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('bf7f5b9f-c0f6-4736-aa5f-9a4fd1c0f687', '2a6f3988-b8cf-40bb-a5fd-7609cb8e4b71', 'Edit', 'Edit', NULL, 3, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('253620b4-bb24-4890-9a14-5d82a7f5bc6c', 'f12de5db-5fce-4a79-89c2-41c63a8af4eb', 'View', 'View', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('8b8e6a6d-c4cb-4e92-8b71-948b6e0f5a61', 'f12de5db-5fce-4a79-89c2-41c63a8af4eb', 'Execute', 'Execute', NULL, 2, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('f2e11709-7588-4250-bcad-479b1505a28e', '7e9fb8f9-8770-4f05-b909-0dc4f18f9245', 'View', 'View', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('524a8ca5-aed7-41e5-bf11-7155a0aeb8b3', '7e9fb8f9-8770-4f05-b909-0dc4f18f9245', 'Create', 'Create', NULL, 2, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('c2776766-afae-4c42-801f-a11e4f7ae635', '7e9fb8f9-8770-4f05-b909-0dc4f18f9245', 'Edit', 'Edit', NULL, 3, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('e55c58dd-ec74-4f27-84a1-b73a18c59ad1', '7e9fb8f9-8770-4f05-b909-0dc4f18f9245', 'Close', 'Close', NULL, 4, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('4f36284d-050b-4adf-9820-170f8de8f3ca', 'f132dccb-2bae-4c34-b28c-56e631973876', 'View', 'View', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('4fd35beb-3207-4d72-9388-94bb4a7ccfe6', 'b5be19b4-ff12-407a-91ee-d6f1f0f8cb20', 'View', 'View', NULL, 1, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
('455b1548-bcb9-48d5-81ce-ac4f31ca3da5', 'b5be19b4-ff12-407a-91ee-d6f1f0f8cb20', 'Edit', 'Edit', NULL, 2, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

INSERT INTO RoleModuleAuthorization (Id, RoleId, ModuleId, Authorized, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), rc.Id, mc.Id,
CASE
    WHEN rc.Code = 'SuperAdmin' THEN 1
    WHEN rc.Code = 'Operators' AND mc.Code IN ('ExcelUploads','LabelVerification','PackingLists') THEN 1
    WHEN rc.Code = 'Managers' THEN 1
    ELSE 0
END,
CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM RoleCatalog rc
CROSS JOIN ModuleCatalog mc;

INSERT INTO RoleModuleActionAuthorization (Id, RoleId, ModuleActionId, Authorized, CreatedAtUtc, UpdatedAtUtc)
SELECT lower(hex(randomblob(16))), rc.Id, mac.Id,
CASE
    WHEN rc.Code = 'SuperAdmin' THEN 1
    WHEN rc.Code = 'Operators' AND (
        (mc.Code = 'ExcelUploads' AND mac.Code IN ('View','Upload')) OR
        (mc.Code = 'LabelVerification' AND mac.Code IN ('View','Execute')) OR
        (mc.Code = 'PackingLists' AND mac.Code IN ('View','Create','Edit'))
    ) THEN 1
    WHEN rc.Code = 'Managers' AND (
        (mc.Code = 'UsersAdministration' AND mac.Code IN ('View')) OR
        (mc.Code = 'ExcelUploads' AND mac.Code IN ('View')) OR
        (mc.Code = 'PartsCatalog' AND mac.Code IN ('View','Edit')) OR
        (mc.Code = 'LabelVerification' AND mac.Code IN ('View')) OR
        (mc.Code = 'PackingLists' AND mac.Code IN ('View','Close')) OR
        (mc.Code = 'AuditTrail' AND mac.Code IN ('View')) OR
        (mc.Code = 'SystemConfiguration' AND mac.Code IN ('View'))
    ) THEN 1
    ELSE 0
END,
CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
FROM RoleCatalog rc
CROSS JOIN ModuleActionCatalog mac
INNER JOIN ModuleCatalog mc ON mc.Id = mac.ModuleId;

INSERT INTO SystemUserRole (Id, SystemUserId, RoleId, IsPrimary, AssignedAtUtc, AssignedByUserId)
SELECT lower(hex(randomblob(16))), su.Id, rc.Id,
    CASE
        WHEN lower(trim(json_extract(su.RolesJson, '$[0]'))) = lower(rc.Code) THEN 1
        ELSE 0
    END,
    CURRENT_TIMESTAMP,
    NULL
FROM SystemUsers su
INNER JOIN json_each(CASE WHEN json_valid(su.RolesJson) THEN su.RolesJson ELSE '[]' END) je
INNER JOIN RoleCatalog rc ON lower(trim(je.value)) = lower(rc.Code)
WHERE NOT EXISTS (
    SELECT 1 FROM SystemUserRole sur
    WHERE sur.SystemUserId = su.Id AND sur.RoleId = rc.Id
);

INSERT INTO SystemUserRole (Id, SystemUserId, RoleId, IsPrimary, AssignedAtUtc, AssignedByUserId)
SELECT lower(hex(randomblob(16))), su.Id, rc.Id, 1, CURRENT_TIMESTAMP, NULL
FROM SystemUsers su
INNER JOIN RoleCatalog rc ON rc.Code = 'Operators'
WHERE NOT EXISTS (
    SELECT 1 FROM SystemUserRole sur
    WHERE sur.SystemUserId = su.Id
);
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleModuleActionAuthorization");

            migrationBuilder.DropTable(
                name: "RoleModuleAuthorization");

            migrationBuilder.DropTable(
                name: "SystemUserRole");

            migrationBuilder.DropTable(
                name: "ModuleActionCatalog");

            migrationBuilder.DropTable(
                name: "RoleCatalog");

            migrationBuilder.DropTable(
                name: "ModuleCatalog");
        }
    }
}
