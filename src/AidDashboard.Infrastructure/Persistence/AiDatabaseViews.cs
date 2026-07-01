using System.Text;
using AidDashboard.Domain.Accounts;
using AidDashboard.Domain.Reference;
using AidDashboard.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Persistence;

/// <summary>
/// Creates the read-only, flattened SQL views the AI assistant queries. The views resolve all
/// reference ids to their names, translate the enum columns to readable text, expose only the
/// business data (never the Users table, password hashes or audit columns) and add the two
/// computed automation figures used on the dashboard. Recreated on startup so they always track
/// the current schema and enum values.
/// </summary>
public static class AiDatabaseViews
{
    /// <summary>The only object names the assistant is allowed to query (enforced by the query guard).</summary>
    public static readonly IReadOnlySet<string> ViewNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "v_account", "v_account_multiselect", "v_automation", "v_opportunity", "v_sla_kpi",
        "v_service", "v_support_hours", "v_staffing", "v_contractual_language", "v_country_device", "v_reference",
    };

    /// <summary>Builds a SQL CASE that maps an integer enum column to the enum member name.</summary>
    private static string EnumCase(string column, Type enumType)
    {
        var builder = new StringBuilder("CASE ").Append(column);
        foreach (var value in Enum.GetValues(enumType))
        {
            builder.Append($" WHEN {(int)value!} THEN '{value}'");
        }
        return builder.Append(" END").ToString();
    }

    /// <summary>Maps a 0/1 (nullable) column to 'No'/'Yes' (or NULL when blank).</summary>
    private static string BoolCase(string column) => $"CASE {column} WHEN 1 THEN 'Yes' WHEN 0 THEN 'No' ELSE NULL END";

    /// <summary>Whole months from a yyyy-MM-dd date column to today, clamped to zero.</summary>
    private static string MonthsSince(string dateColumn) =>
        $"MAX(0, (CAST(strftime('%Y','now') AS INTEGER) - CAST(strftime('%Y',{dateColumn}) AS INTEGER)) * 12 "
        + $"+ (CAST(strftime('%m','now') AS INTEGER) - CAST(strftime('%m',{dateColumn}) AS INTEGER)))";

    private static Dictionary<string, string> Definitions()
    {
        var costReal = "CAST(au.CostOfImplementationOneTime AS REAL)";
        var runReal = "CAST(au.RunningCostMonthly AS REAL)";

        return new Dictionary<string, string>
        {
            ["v_account"] = $@"
                SELECT a.Name AS account, a.RegionalOwnership AS regional_ownership,
                    a.GlobalSm AS global_sm, a.GlobalSdm AS global_sdm, a.EmeaLtsSdm AS emea_lts_sdm,
                    a.NaLtsSdm AS na_lts_sdm, a.EmeaSm AS emea_sm, a.EmeaSdm AS emea_sdm, a.NaSdm AS na_sdm,
                    a.ApacSdm AS apac_sdm, a.Tsm AS tsm, a.BuLeader AS bu_leader,
                    a.TransformationManager AS transformation_manager, a.AccountManagerCrd AS account_manager_crd,
                    a.LaunchDate AS launch_date, a.ContractEndDate AS contract_end_date,
                    rat.Name AS account_type, {BoolCase("a.FlagshipAccount")} AS flagship_account,
                    rind.Name AS industry, rhq.Name AS headquarter_country,
                    a.ContractSupervisor AS contract_supervisor, a.NoOfUsersSupported AS no_of_users_supported,
                    {BoolCase("a.SharingConstraints")} AS sharing_constraints, rconn.Name AS connectivity,
                    a.TelecomCountry AS telecom_country, rtel.Name AS telecom,
                    {BoolCase("a.ClientHardware")} AS client_hardware, ritsm.Name AS itsm_tool, rman.Name AS managed_by,
                    a.ProjectTrainingDurationDays AS project_training_duration_days, a.SupportChannels AS support_channels,
                    {BoolCase("a.WorkFromHomeApproved")} AS work_from_home_approved
                FROM Accounts a
                LEFT JOIN ReferenceItems rat ON rat.Id = a.AccountTypeRefId
                LEFT JOIN ReferenceItems rind ON rind.Id = a.IndustryRefId
                LEFT JOIN ReferenceItems rhq ON rhq.Id = a.HeadquarterCountryRefId
                LEFT JOIN ReferenceItems rconn ON rconn.Id = a.ConnectivityRefId
                LEFT JOIN ReferenceItems rtel ON rtel.Id = a.TelecomRefId
                LEFT JOIN ReferenceItems ritsm ON ritsm.Id = a.ItsmToolRefId
                LEFT JOIN ReferenceItems rman ON rman.Id = a.ManagedByRefId",

            ["v_account_multiselect"] = $@"
                SELECT a.Name AS account, {EnumCase("m.Field", typeof(MultiSelectField))} AS field, r.Name AS value
                FROM AccountMultiSelectValues m
                JOIN Accounts a ON a.Id = m.AccountId
                LEFT JOIN ReferenceItems r ON r.Id = m.ReferenceItemId",

            ["v_automation"] = $@"
                SELECT a.Name AS account, ra.Name AS automation, rc.Name AS category, rg.Name AS goal,
                    re.Name AS environment, {BoolCase("ap.AiUsed")} AS ai_used, rd.Name AS delivered_by,
                    au.DeploymentDate AS deployment_date,
                    {costReal} AS cost_of_implementation_one_time,
                    {runReal} AS running_cost_monthly,
                    au.EfficiencyImpactFtePerMonth AS efficiency_fte_per_month,
                    (COALESCE({costReal},0) + {MonthsSince("au.DeploymentDate")} * COALESCE({runReal},0)) AS cost_total,
                    (1500 * COALESCE(au.EfficiencyImpactFtePerMonth,0)) AS estimated_saving_monthly,
                    au.Details AS details
                FROM Automations au
                JOIN Accounts a ON a.Id = au.AccountId
                LEFT JOIN ReferenceItems ra ON ra.Id = au.AutomationRefId
                LEFT JOIN AutomationProfiles ap ON ap.AutomationRefId = au.AutomationRefId
                LEFT JOIN ReferenceItems rc ON rc.Id = ap.CategoryRefId
                LEFT JOIN ReferenceItems rg ON rg.Id = ap.GoalRefId
                LEFT JOIN ReferenceItems re ON re.Id = ap.EnvironmentRefId
                LEFT JOIN ReferenceItems rd ON rd.Id = au.DeliveredByRefId",

            ["v_opportunity"] = @"
                SELECT a.Name AS account, o.Name AS opportunity, rt.Name AS type, rs.Name AS related_service,
                    rst.Name AS status, CAST(o.EstimatedMonthlyValue AS REAL) AS estimated_monthly_value,
                    o.EstimatedContractDurationMonths AS estimated_duration_months, rd.Name AS delivered_by, o.Details AS details
                FROM Opportunities o
                JOIN Accounts a ON a.Id = o.AccountId
                LEFT JOIN ReferenceItems rt ON rt.Id = o.OpportunityTypeRefId
                LEFT JOIN ReferenceItems rs ON rs.Id = o.RelatedServiceRefId
                LEFT JOIN ReferenceItems rst ON rst.Id = o.StatusRefId
                LEFT JOIN ReferenceItems rd ON rd.Id = o.DeliveredByRefId",

            ["v_sla_kpi"] = $@"
                SELECT a.Name AS account, s.Name AS name, rt.Name AS type, s.Description AS description,
                    s.Formula AS formula, s.TargetPercent AS target_percent, rm.Name AS measurement_type,
                    {BoolCase("s.CanBeReported")} AS can_be_reported, {BoolCase("s.FinancialPenalties")} AS financial_penalties,
                    {BoolCase("s.Bonus")} AS bonus
                FROM SlaKpis s
                JOIN Accounts a ON a.Id = s.AccountId
                LEFT JOIN ReferenceItems rt ON rt.Id = s.TypeRefId
                LEFT JOIN ReferenceItems rm ON rm.Id = s.MeasurementTypeRefId",

            ["v_service"] = $@"
                SELECT a.Name AS account, parent.Name AS category, sci.Name AS service,
                    se.ContractEndDate AS contract_end_date, {BoolCase("se.ExistsInCompany")} AS exists_in_company,
                    {BoolCase("se.ProvidedByStefaniniGroup")} AS provided_by_stefanini, rp.Name AS provided_by,
                    {BoolCase("se.OpportunityToProvide")} AS opportunity_to_provide, se.Details AS details
                FROM AccountServiceEntries se
                JOIN Accounts a ON a.Id = se.AccountId
                LEFT JOIN ServiceCatalogItems sci ON sci.Id = se.ServiceItemId
                LEFT JOIN ServiceCatalogItems parent ON parent.Id = sci.ParentId
                LEFT JOIN ReferenceItems rp ON rp.Id = se.ProvidedByRefId",

            ["v_support_hours"] = $@"
                SELECT a.Name AS account, rl.Name AS language, sh.FromMondayFriday AS from_mon_fri,
                    sh.ToMondayFriday AS to_mon_fri, {EnumCase("sh.Coverage", typeof(SupportCoverage))} AS coverage,
                    {BoolCase("sh.OnCallInterpretDhs")} AS on_call_interpret_dhs, {BoolCase("sh.Sophie")} AS sophie
                FROM SupportHours sh
                JOIN Accounts a ON a.Id = sh.AccountId
                LEFT JOIN ReferenceItems rl ON rl.Id = sh.LanguageRefId",

            ["v_staffing"] = $@"
                SELECT a.Name AS account, {EnumCase("sc.ModelType", typeof(StaffingModelType))} AS model_type,
                    rl.Name AS location, rr.Name AS role, sc.Value AS value
                FROM StaffingCells sc
                JOIN Accounts a ON a.Id = sc.AccountId
                LEFT JOIN ReferenceItems rl ON rl.Id = sc.LocationRefId
                LEFT JOIN ReferenceItems rr ON rr.Id = sc.RoleRefId",

            ["v_contractual_language"] = @"
                SELECT a.Name AS account, rl.Name AS location, rlang.Name AS language, c.Value AS value
                FROM ContractualLanguageCells c
                JOIN Accounts a ON a.Id = c.AccountId
                LEFT JOIN ReferenceItems rl ON rl.Id = c.LocationRefId
                LEFT JOIN ReferenceItems rlang ON rlang.Id = c.LanguageRefId",

            ["v_country_device"] = @"
                SELECT a.Name AS account, rc.Name AS country, ac.NoOfUsers AS no_of_users,
                    ac.NoOfLtsStaff AS no_of_lts_staff, rd.Name AS device, cdc.Value AS device_count
                FROM AccountCountries ac
                JOIN Accounts a ON a.Id = ac.AccountId
                LEFT JOIN ReferenceItems rc ON rc.Id = ac.CountryRefId
                LEFT JOIN CountryDeviceCells cdc ON cdc.AccountCountryId = ac.Id
                LEFT JOIN ReferenceItems rd ON rd.Id = cdc.DeviceRefId",

            ["v_reference"] = $@"
                SELECT {EnumCase("Type", typeof(ReferenceType))} AS type, Name AS name, {BoolCase("IsActive")} AS is_active
                FROM ReferenceItems",
        };
    }

    /// <summary>(Re)creates every assistant view. Safe to call on every startup.</summary>
    public static async Task EnsureAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        foreach (var (name, select) in Definitions())
        {
            // The SQL here is built entirely from our own constants and enum members (no user input),
            // so the interpolation is safe; suppress the injection analyzer for these DDL statements.
#pragma warning disable EF1002
            await dbContext.Database.ExecuteSqlRawAsync($"DROP VIEW IF EXISTS {name};", cancellationToken);
            await dbContext.Database.ExecuteSqlRawAsync($"CREATE VIEW {name} AS {select};", cancellationToken);
#pragma warning restore EF1002
        }
    }
}
