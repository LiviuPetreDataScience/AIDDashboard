using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AidDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    RegionalOwnership = table.Column<string>(type: "TEXT", nullable: true),
                    GlobalSm = table.Column<string>(type: "TEXT", nullable: true),
                    GlobalSdm = table.Column<string>(type: "TEXT", nullable: true),
                    EmeaLtsSdm = table.Column<string>(type: "TEXT", nullable: true),
                    NaLtsSdm = table.Column<string>(type: "TEXT", nullable: true),
                    EmeaSm = table.Column<string>(type: "TEXT", nullable: true),
                    EmeaSdm = table.Column<string>(type: "TEXT", nullable: true),
                    NaSdm = table.Column<string>(type: "TEXT", nullable: true),
                    ApacSdm = table.Column<string>(type: "TEXT", nullable: true),
                    Tsm = table.Column<string>(type: "TEXT", nullable: true),
                    BuLeader = table.Column<string>(type: "TEXT", nullable: true),
                    TransformationManager = table.Column<string>(type: "TEXT", nullable: true),
                    AccountManagerCrd = table.Column<string>(type: "TEXT", nullable: true),
                    LaunchDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ContractEndDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    AccountTypeRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    FlagshipAccount = table.Column<bool>(type: "INTEGER", nullable: true),
                    IndustryRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    HeadquarterCountryRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    ContractSupervisor = table.Column<string>(type: "TEXT", nullable: true),
                    NoOfUsersSupported = table.Column<long>(type: "INTEGER", nullable: true),
                    SharingConstraints = table.Column<bool>(type: "INTEGER", nullable: true),
                    ConnectivityRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    TelecomCountry = table.Column<string>(type: "TEXT", nullable: true),
                    TelecomRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    ClientHardware = table.Column<bool>(type: "INTEGER", nullable: true),
                    ItsmToolRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    ManagedByRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProjectTrainingDurationDays = table.Column<int>(type: "INTEGER", nullable: true),
                    SupportChannels = table.Column<string>(type: "TEXT", nullable: true),
                    WorkFromHomeApproved = table.Column<bool>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    ChangesJson = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReferenceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferenceItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountCountries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    CountryRefId = table.Column<int>(type: "INTEGER", nullable: false),
                    NoOfUsers = table.Column<long>(type: "INTEGER", nullable: true),
                    NoOfLtsStaff = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountCountries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountCountries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountMultiSelectValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Field = table.Column<int>(type: "INTEGER", nullable: false),
                    ReferenceItemId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountMultiSelectValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountMultiSelectValues_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Automations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    DeploymentDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    CostOfImplementationOneTime = table.Column<decimal>(type: "TEXT", nullable: true),
                    RunningCostMonthly = table.Column<decimal>(type: "TEXT", nullable: true),
                    EfficiencyImpactFtePerMonth = table.Column<double>(type: "REAL", nullable: true),
                    DeliveredByRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractualLanguageCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationRefId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageRefId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractualLanguageCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractualLanguageCells_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    OpportunityTypeRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    RelatedServiceRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    EstimatedMonthlyValue = table.Column<decimal>(type: "TEXT", nullable: true),
                    EstimatedContractDurationMonths = table.Column<int>(type: "INTEGER", nullable: true),
                    DeliveredByRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlaKpis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    TypeRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Formula = table.Column<string>(type: "TEXT", nullable: true),
                    TargetPercent = table.Column<string>(type: "TEXT", nullable: true),
                    MeasurementTypeRefId = table.Column<int>(type: "INTEGER", nullable: true),
                    CanBeReported = table.Column<bool>(type: "INTEGER", nullable: false),
                    FinancialPenalties = table.Column<bool>(type: "INTEGER", nullable: false),
                    Bonus = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaKpis_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffingCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModelType = table.Column<int>(type: "INTEGER", nullable: false),
                    LocationRefId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleRefId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffingCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffingCells_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageRefId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromMondayFriday = table.Column<string>(type: "TEXT", nullable: true),
                    ToMondayFriday = table.Column<string>(type: "TEXT", nullable: true),
                    Coverage = table.Column<int>(type: "INTEGER", nullable: false),
                    OnCallInterpretDhs = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sophie = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportHours_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CountryDeviceCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountCountryId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceRefId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryDeviceCells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountryDeviceCells_AccountCountries_AccountCountryId",
                        column: x => x.AccountCountryId,
                        principalTable: "AccountCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountCountries_AccountId_CountryRefId",
                table: "AccountCountries",
                columns: new[] { "AccountId", "CountryRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountMultiSelectValues_AccountId_Field_ReferenceItemId",
                table: "AccountMultiSelectValues",
                columns: new[] { "AccountId", "Field", "ReferenceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Name",
                table: "Accounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Automations_AccountId",
                table: "Automations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_AccountId",
                table: "ChangeLogs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogs_EntityType_EntityId",
                table: "ChangeLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractualLanguageCells_AccountId_LocationRefId_LanguageRefId",
                table: "ContractualLanguageCells",
                columns: new[] { "AccountId", "LocationRefId", "LanguageRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryDeviceCells_AccountCountryId_DeviceRefId",
                table: "CountryDeviceCells",
                columns: new[] { "AccountCountryId", "DeviceRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_AccountId",
                table: "Opportunities",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceItems_Type",
                table: "ReferenceItems",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ReferenceItems_Type_Name",
                table: "ReferenceItems",
                columns: new[] { "Type", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlaKpis_AccountId",
                table: "SlaKpis",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffingCells_AccountId_ModelType_LocationRefId_RoleRefId",
                table: "StaffingCells",
                columns: new[] { "AccountId", "ModelType", "LocationRefId", "RoleRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportHours_AccountId_LanguageRefId",
                table: "SupportHours",
                columns: new[] { "AccountId", "LanguageRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountMultiSelectValues");

            migrationBuilder.DropTable(
                name: "Automations");

            migrationBuilder.DropTable(
                name: "ChangeLogs");

            migrationBuilder.DropTable(
                name: "ContractualLanguageCells");

            migrationBuilder.DropTable(
                name: "CountryDeviceCells");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropTable(
                name: "ReferenceItems");

            migrationBuilder.DropTable(
                name: "SlaKpis");

            migrationBuilder.DropTable(
                name: "StaffingCells");

            migrationBuilder.DropTable(
                name: "SupportHours");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AccountCountries");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
