using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AidDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutomationAsReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Automations");

            migrationBuilder.AddColumn<int>(
                name: "AutomationRefId",
                table: "Automations",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutomationRefId",
                table: "Automations");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Automations",
                type: "TEXT",
                nullable: true);
        }
    }
}
