namespace AidDashboard.Domain.Accounts;

/// <summary>
/// A numeric quantity for one device type, for one country row, on the
/// Countries / Users / Devices tab (e.g. number of Laptops in Japan).
/// </summary>
public class CountryDeviceCell
{
    public int Id { get; set; }
    public int AccountCountryId { get; set; }
    public int DeviceRefId { get; set; }
    public double Value { get; set; }
}
