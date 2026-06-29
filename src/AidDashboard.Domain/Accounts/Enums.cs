namespace AidDashboard.Domain.Accounts;

/// <summary>The two staffing models share an identical layout; this distinguishes them.</summary>
public enum StaffingModelType
{
    Initial = 0,
    LatestApproved = 1
}

/// <summary>Support-hours coverage (the three mutually-exclusive radios on the Support Hours tab).</summary>
public enum SupportCoverage
{
    None = 0,
    EightByFive = 1,
    TwentyFourFive = 2,
    TwentyFourSeven = 3
}

/// <summary>
/// The multi-select fields on the Overall tab. All multi-select selections are stored
/// in one table (<see cref="AccountMultiSelectValue"/>) keyed by this field.
/// </summary>
public enum MultiSelectField
{
    ContractType = 0,
    BillingModel = 1,
    BillingModelLts = 2,
    BillingModelInfra = 3,
    InScope = 4,
    TechnologySupported = 5,
    DevicesSupported = 6
}
