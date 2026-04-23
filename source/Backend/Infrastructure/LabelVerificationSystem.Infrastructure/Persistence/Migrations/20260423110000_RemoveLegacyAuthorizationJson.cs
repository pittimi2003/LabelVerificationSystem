using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerificationSystem.Infrastructure.Persistence.Migrations;

[Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(AppDbContext))]
[Migration("20260423110000_RemoveLegacyAuthorizationJson")]
public partial class RemoveLegacyAuthorizationJson : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            PRAGMA foreign_keys = OFF;

            CREATE TABLE "SystemUsers_new" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_SystemUsers" PRIMARY KEY,
                "UserId" TEXT NOT NULL,
                "Username" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "Email" TEXT NULL,
                "IsActive" INTEGER NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL
            );

            INSERT INTO "SystemUsers_new" ("Id", "UserId", "Username", "DisplayName", "Email", "IsActive", "CreatedAtUtc", "UpdatedAtUtc")
            SELECT "Id", "UserId", "Username", "DisplayName", "Email", "IsActive", "CreatedAtUtc", "UpdatedAtUtc"
            FROM "SystemUsers";

            DROP TABLE "SystemUsers";
            ALTER TABLE "SystemUsers_new" RENAME TO "SystemUsers";

            CREATE INDEX "IX_SystemUsers_Email" ON "SystemUsers" ("Email");
            CREATE INDEX "IX_SystemUsers_IsActive" ON "SystemUsers" ("IsActive");
            CREATE UNIQUE INDEX "IX_SystemUsers_UserId" ON "SystemUsers" ("UserId");
            CREATE UNIQUE INDEX "IX_SystemUsers_Username" ON "SystemUsers" ("Username");

            PRAGMA foreign_keys = ON;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            PRAGMA foreign_keys = OFF;

            CREATE TABLE "SystemUsers_old" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_SystemUsers" PRIMARY KEY,
                "UserId" TEXT NOT NULL,
                "Username" TEXT NOT NULL,
                "DisplayName" TEXT NOT NULL,
                "Email" TEXT NULL,
                "IsActive" INTEGER NOT NULL,
                "RolesJson" TEXT NOT NULL,
                "PermissionsJson" TEXT NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL
            );

            INSERT INTO "SystemUsers_old" ("Id", "UserId", "Username", "DisplayName", "Email", "IsActive", "RolesJson", "PermissionsJson", "CreatedAtUtc", "UpdatedAtUtc")
            SELECT "Id", "UserId", "Username", "DisplayName", "Email", "IsActive", '[]', '[]', "CreatedAtUtc", "UpdatedAtUtc"
            FROM "SystemUsers";

            DROP TABLE "SystemUsers";
            ALTER TABLE "SystemUsers_old" RENAME TO "SystemUsers";

            CREATE INDEX "IX_SystemUsers_Email" ON "SystemUsers" ("Email");
            CREATE INDEX "IX_SystemUsers_IsActive" ON "SystemUsers" ("IsActive");
            CREATE UNIQUE INDEX "IX_SystemUsers_UserId" ON "SystemUsers" ("UserId");
            CREATE UNIQUE INDEX "IX_SystemUsers_Username" ON "SystemUsers" ("Username");

            PRAGMA foreign_keys = ON;
            """);
    }
}
