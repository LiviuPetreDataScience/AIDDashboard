namespace AidDashboard.Domain.Reference;

/// <summary>
/// Discriminator for the single reference-table store. Each value is one logical
/// reference list that admins can maintain and that the tabs draw their options from.
/// </summary>
public enum ReferenceType
{
    Location = 0,
    StaffingRole = 1,
    ContractualLanguage = 2,
    SupportHoursLanguage = 3,
    Device = 4,
    Country = 5,
    ServiceTower = 6,
    OpportunityType = 7,
    OpportunityStatus = 8,
    RelatedService = 9,
    SlaType = 10,
    MeasurementType = 11,
    AccountType = 12,
    Connectivity = 13,
    Telecom = 14,
    ManagedBy = 15,
    ItsmTool = 16,
    ContractType = 17,
    BillingModel = 18,
    TechnologySupported = 19,
    Industry = 20
}
