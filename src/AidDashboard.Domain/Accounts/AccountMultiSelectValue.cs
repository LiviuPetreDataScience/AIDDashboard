namespace AidDashboard.Domain.Accounts;

/// <summary>
/// One selected option for a multi-select Overall field (e.g. one chosen Billing model).
/// </summary>
public class AccountMultiSelectValue
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public MultiSelectField Field { get; set; }
    public int ReferenceItemId { get; set; }
}
