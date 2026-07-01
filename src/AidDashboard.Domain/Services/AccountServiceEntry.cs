using AidDashboard.Domain.Common;

namespace AidDashboard.Domain.Services;

/// <summary>
/// One account's data for a single service (a row on the Services tab). There is at most one
/// entry per (account, service). Services with no entry are simply "not filled in" for the account.
/// </summary>
public class AccountServiceEntry : AuditableEntity
{
    public int AccountId { get; set; }
    /// <summary>The leaf <see cref="ServiceCatalogItem"/> (kind Service) this entry is for.</summary>
    public int ServiceItemId { get; set; }

    public DateOnly? ContractEndDate { get; set; }
    public bool? ExistsInCompany { get; set; }
    public bool? ProvidedByStefaniniGroup { get; set; }
    /// <summary>Competitor/provider reference id used when not provided by Stefanini Group.</summary>
    public int? ProvidedByRefId { get; set; }
    public bool? OpportunityToProvide { get; set; }
    public string? Details { get; set; }
}
