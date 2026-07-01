using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AidDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddServicesCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountServiceEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServiceItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContractEndDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ExistsInCompany = table.Column<bool>(type: "INTEGER", nullable: true),
                    ProvidedByStefaniniGroup = table.Column<bool>(type: "INTEGER", nullable: true),
                    ProvidedByRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    OpportunityToProvide = table.Column<bool>(type: "INTEGER", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountServiceEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountServiceEntries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCatalogItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCatalogItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceCatalogItems_ServiceCatalogItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ServiceCatalogItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountServiceEntries_AccountId_ServiceItemId",
                table: "AccountServiceEntries",
                columns: new[] { "AccountId", "ServiceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCatalogItems_ParentId",
                table: "ServiceCatalogItems",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountServiceEntries");

            migrationBuilder.DropTable(
                name: "ServiceCatalogItems");
        }
    }
}
