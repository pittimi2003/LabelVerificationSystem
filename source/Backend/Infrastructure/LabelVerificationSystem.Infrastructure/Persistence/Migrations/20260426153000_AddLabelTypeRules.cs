using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerificationSystem.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260426153000_AddLabelTypeRules")]
public partial class AddLabelTypeRules : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "LabelTypeRules",
            columns: table => new
            {
                Id = table.Column<string>(type: "TEXT", nullable: false),
                LabelTypeId = table.Column<string>(type: "TEXT", nullable: false),
                ColumnName = table.Column<string>(type: "TEXT", nullable: false),
                ExpectedValue = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LabelTypeRules", x => x.Id);
                table.ForeignKey(
                    name: "FK_LabelTypeRules_LabelTypes_LabelTypeId",
                    column: x => x.LabelTypeId,
                    principalTable: "LabelTypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LabelTypeRules_LabelTypeId_ColumnName",
            table: "LabelTypeRules",
            columns: new[] { "LabelTypeId", "ColumnName" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "LabelTypeRules");
    }
}
