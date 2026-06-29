namespace AidDashboard.Domain.Accounts;

/// <summary>
/// A country supported by an account (a row on the Countries / Users / Devices tab).
/// Countries are added per account (unlike Locations, which are a fixed set), so membership
/// is explicit. Device quantities for the country hang off this row.
/// </summary>
public class AccountCountry
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int CountryRefId { get; set; }

    // Two fixed leading columns on the tab (not device types).
    public long? NoOfUsers { get; set; }
    public int? NoOfLtsStaff { get; set; }

    public List<CountryDeviceCell> DeviceCells { get; set; } = new();
}
