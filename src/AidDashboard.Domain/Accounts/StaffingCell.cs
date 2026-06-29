namespace AidDashboard.Domain.Accounts;

/// <summary>
/// A single numeric cell in a staffing model (Location x Role), for either the
/// Initial or the Latest Approved model. Rows are Locations, columns are Staffing Roles.
/// </summary>
public class StaffingCell
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public StaffingModelType ModelType { get; set; }
    public int LocationRefId { get; set; }
    public int RoleRefId { get; set; }
    public double Value { get; set; }
}
