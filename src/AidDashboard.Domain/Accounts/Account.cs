using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Accounts;

/// <summary>
/// The central entity. All data in the application is recorded per Account.
/// This entity holds the "Overall" tab fields; the other tabs hang off it as child collections.
/// Single-select reference fields are stored as nullable reference-item ids;
/// multi-select fields live in <see cref="MultiSelectValues"/>.
/// </summary>
public class Account : AuditableEntity
{
    public string Name { get; set; } = default!;

    // ---- SG specifics (free text / dates) ----
    public string? RegionalOwnership { get; set; }
    public string? GlobalSm { get; set; }
    public string? GlobalSdm { get; set; }
    public string? EmeaLtsSdm { get; set; }
    public string? NaLtsSdm { get; set; }
    public string? EmeaSm { get; set; }
    public string? EmeaSdm { get; set; }
    public string? NaSdm { get; set; }
    public string? ApacSdm { get; set; }
    public string? Tsm { get; set; }
    public string? BuLeader { get; set; }
    public string? TransformationManager { get; set; }
    public string? AccountManagerCrd { get; set; }
    public DateOnly? LaunchDate { get; set; }
    public DateOnly? ContractEndDate { get; set; }

    // ---- SG specifics (reference / yes-no) ----
    public int? AccountTypeRefId { get; set; }          // Type of account (ITO/DHS/BPO)
    public bool? FlagshipAccount { get; set; }

    // ---- Client specifics ----
    public int? IndustryRefId { get; set; }
    public int? HeadquarterCountryRefId { get; set; }
    public string? ContractSupervisor { get; set; }      // Counterparty SM
    public long? NoOfUsersSupported { get; set; }
    public bool? SharingConstraints { get; set; }

    // ---- Technology ----
    public int? ConnectivityRefId { get; set; }
    public string? TelecomCountry { get; set; }          // free text "Telecom" country value
    public int? TelecomRefId { get; set; }               // Telecom (EMEA/US/Client)
    public bool? ClientHardware { get; set; }            // Hardware: Client hardware (Y/N)
    public int? ItsmToolRefId { get; set; }
    public int? ManagedByRefId { get; set; }

    // ---- Other ----
    public int? ProjectTrainingDurationDays { get; set; }
    public string? SupportChannels { get; set; }
    public bool? WorkFromHomeApproved { get; set; }

    // ---- Navigation ----
    public List<AccountMultiSelectValue> MultiSelectValues { get; set; } = new();
    public List<StaffingCell> StaffingCells { get; set; } = new();
    public List<ContractualLanguageCell> ContractualLanguageCells { get; set; } = new();
    public List<AccountCountry> Countries { get; set; } = new();
    public List<Automation> Automations { get; set; } = new();
    public List<Opportunity> Opportunities { get; set; } = new();
    public List<SlaKpi> SlaKpis { get; set; } = new();
    public List<SupportHour> SupportHours { get; set; } = new();
}
