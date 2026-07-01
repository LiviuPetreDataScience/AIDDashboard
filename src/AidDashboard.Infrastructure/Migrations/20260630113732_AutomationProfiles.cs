using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AidDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutomationProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomationProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AutomationRefId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    GoalRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    EnvironmentRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    AiUsed = table.Column<bool>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationProfiles_AutomationRefId",
                table: "AutomationProfiles",
                column: "AutomationRefId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationProfiles");
        }
    }
}
