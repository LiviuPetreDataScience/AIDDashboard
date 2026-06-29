using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Reference;

namespace AidDashboard.Infrastructure.ImportExport.Providers;

/// <summary>Import/export mapping for the Countries / Users / Devices Supported tab.</summary>
public class CountriesDevicesTableProvider : ITabTableProvider
{
    private readonly ICountryDeviceService _countryDeviceService;
    private readonly IReferenceService _referenceService;

    public CountriesDevicesTableProvider(ICountryDeviceService countryDeviceService, IReferenceService referenceService)
    {
        _countryDeviceService = countryDeviceService;
        _referenceService = referenceService;
    }

    public string TabKey => "countries-devices";
    public string DisplayTitle => "Countries Users Devices";

    public async Task<TableData> ExportAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var countries = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Country, true, cancellationToken));
        var devices = await _referenceService.GetByTypeAsync(ReferenceType.Device, false, cancellationToken);
        var rows = await _countryDeviceService.GetAsync(accountId, cancellationToken);

        var headers = BuildHeaders(devices);

        var tableRows = new List<IReadOnlyList<string?>>();
        foreach (var row in rows)
        {
            var valueByDevice = row.Devices.ToDictionary(device => device.DeviceRefId, device => device.Value);

            var cells = new List<string?>
            {
                countries.NameOf(row.CountryRefId),
                CellParsers.FormatNumber(row.NoOfUsers),
                CellParsers.FormatNumber(row.NoOfLtsStaff),
            };
            foreach (var device in devices)
            {
                cells.Add(valueByDevice.TryGetValue(device.Id, out var value) ? CellParsers.FormatNumber((double?)value) : string.Empty);
            }
            tableRows.Add(cells);
        }

        return new TableData(headers, tableRows);
    }

    public async Task<TableData> TemplateAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _referenceService.GetByTypeAsync(ReferenceType.Device, false, cancellationToken);
        return TableData.TemplateOf(BuildHeaders(devices));
    }

    public async Task ImportAsync(int accountId, TableData data, CancellationToken cancellationToken = default)
    {
        var countryLookup = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Country, true, cancellationToken));
        var deviceLookup = new ReferenceLookup(await _referenceService.GetByTypeAsync(ReferenceType.Device, true, cancellationToken));

        // Map device columns (everything after the three fixed leading columns) to device ids.
        var deviceColumnIds = new Dictionary<int, int>();
        for (var index = 3; index < data.Headers.Count; index++)
        {
            var deviceId = deviceLookup.IdOf(data.Headers[index]);
            if (deviceId is not null)
            {
                deviceColumnIds[index] = deviceId.Value;
            }
        }

        var rows = new List<CountryRowDto>();
        foreach (var row in data.Rows)
        {
            var countryId = countryLookup.IdOf(row.At(0));
            if (countryId is null)
            {
                continue;
            }

            var deviceValues = new List<CountryDeviceValueDto>();
            foreach (var (columnIndex, deviceId) in deviceColumnIds)
            {
                var value = CellParsers.Double(row.At(columnIndex));
                if (value is not null)
                {
                    deviceValues.Add(new CountryDeviceValueDto(deviceId, value.Value));
                }
            }

            rows.Add(new CountryRowDto
            {
                CountryRefId = countryId.Value,
                NoOfUsers = CellParsers.Long(row.At(1)),
                NoOfLtsStaff = CellParsers.Int(row.At(2)),
                Devices = deviceValues,
            });
        }

        await _countryDeviceService.SaveAsync(accountId, rows, cancellationToken);
    }

    private static List<string> BuildHeaders(IReadOnlyList<ReferenceItemDto> devices)
    {
        var headers = new List<string> { "Country", "No of Users", "No of LTS Staff" };
        headers.AddRange(devices.Select(device => device.Name));
        return headers;
    }
}
