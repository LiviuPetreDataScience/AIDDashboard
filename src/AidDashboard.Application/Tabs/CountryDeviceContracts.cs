namespace AidDashboard.Application.Tabs;

/// <summary>A device quantity within a country row.</summary>
public record CountryDeviceValueDto(int DeviceRefId, double Value);

/// <summary>One country row on the Countries / Users / Devices tab.</summary>
public class CountryRowDto
{
    public int CountryRefId { get; set; }
    public long? NoOfUsers { get; set; }
    public int? NoOfLtsStaff { get; set; }
    public List<CountryDeviceValueDto> Devices { get; set; } = new();
}

/// <summary>Reads and saves the Countries / Users / Devices tab for an account.</summary>
public interface ICountryDeviceService
{
    Task<IReadOnlyList<CountryRowDto>> GetAsync(int accountId, CancellationToken cancellationToken = default);

    Task<bool> SaveAsync(int accountId, IReadOnlyList<CountryRowDto> rows, CancellationToken cancellationToken = default);
}
