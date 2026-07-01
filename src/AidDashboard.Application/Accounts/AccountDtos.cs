namespace AidDashboard.Application.Accounts;

/// <summary>Lightweight account record used by the account picker and the home accounts list.</summary>
public record AccountSummaryDto(int Id, string Name, DateTime? LastUpdatedUtc);

/// <summary>Payload to create a new account.</summary>
public record CreateAccountRequest(string Name);

/// <summary>
/// The full set of "Overall" tab fields for one account. Single-select reference fields are
/// represented by their reference-item id (nullable); multi-select fields are lists of ids.
/// Free-text and yes/no fields map directly. Used for both reading and updating the tab.
/// </summary>
public class AccountOverallDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

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
    public int? AccountTypeRefId { get; set; }
    public bool? FlagshipAccount { get; set; }

    // ---- Client specifics ----
    public int? IndustryRefId { get; set; }
    public int? HeadquarterCountryRefId { get; set; }
    public string? ContractSupervisor { get; set; }
    public long? NoOfUsersSupported { get; set; }
    public bool? SharingConstraints { get; set; }

    // ---- Technology ----
    public int? ConnectivityRefId { get; set; }
    public string? TelecomCountry { get; set; }
    public int? TelecomRefId { get; set; }
    public bool? ClientHardware { get; set; }
    public int? ItsmToolRefId { get; set; }
    public int? ManagedByRefId { get; set; }

    // ---- Other ----
    public int? ProjectTrainingDurationDays { get; set; }
    public string? SupportChannels { get; set; }
    public bool? WorkFromHomeApproved { get; set; }

    // ---- Multi-select fields (lists of reference-item ids) ----
    public List<int> ContractType { get; set; } = new();
    public List<int> BillingModel { get; set; } = new();
    public List<int> BillingModelLts { get; set; } = new();
    public List<int> BillingModelInfra { get; set; } = new();
    public List<int> InScope { get; set; } = new();
    public List<int> TechnologySupported { get; set; } = new();
    public List<int> DevicesSupported { get; set; } = new();
}
