namespace AidDashboard.Domain.Accounts;

/// <summary>
/// A single numeric cell in the Contractual Languages matrix (Location x Language).
/// </summary>
public class ContractualLanguageCell
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int LocationRefId { get; set; }
    public int LanguageRefId { get; set; }
    public double Value { get; set; }
}
