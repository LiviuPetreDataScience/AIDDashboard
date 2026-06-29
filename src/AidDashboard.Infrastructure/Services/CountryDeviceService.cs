using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="ICountryDeviceService"/>
public class CountryDeviceService : ICountryDeviceService
{
    private readonly AppDbContext _dbContext;

    public CountryDeviceService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CountryRowDto>> GetAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var countryRows = await _dbContext.AccountCountries
            .AsNoTracking()
            .Include(country => country.DeviceCells)
            .Where(country => country.AccountId == accountId)
            .ToListAsync(cancellationToken);

        return countryRows
            .Select(country => new CountryRowDto
            {
                CountryRefId = country.CountryRefId,
                NoOfUsers = country.NoOfUsers,
                NoOfLtsStaff = country.NoOfLtsStaff,
                Devices = country.DeviceCells
                    .Select(deviceCell => new CountryDeviceValueDto(deviceCell.DeviceRefId, deviceCell.Value))
                    .ToList()
            })
            .ToList();
    }

    public async Task<bool> SaveAsync(int accountId, IReadOnlyList<CountryRowDto> rows, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Accounts.AnyAsync(account => account.Id == accountId, cancellationToken))
        {
            return false;
        }

        var existingCountryRows = await _dbContext.AccountCountries
            .Include(country => country.DeviceCells)
            .Where(country => country.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var existingByCountry = existingCountryRows.ToDictionary(country => country.CountryRefId);
        var incomingCountryIds = new HashSet<int>();

        foreach (var incomingRow in rows)
        {
            incomingCountryIds.Add(incomingRow.CountryRefId);

            if (!existingByCountry.TryGetValue(incomingRow.CountryRefId, out var countryRow))
            {
                countryRow = new AccountCountry
                {
                    AccountId = accountId,
                    CountryRefId = incomingRow.CountryRefId
                };
                _dbContext.AccountCountries.Add(countryRow);
            }

            countryRow.NoOfUsers = incomingRow.NoOfUsers;
            countryRow.NoOfLtsStaff = incomingRow.NoOfLtsStaff;

            ApplyDeviceCells(countryRow, incomingRow.Devices);
        }

        foreach (var existingRow in existingCountryRows)
        {
            if (!incomingCountryIds.Contains(existingRow.CountryRefId))
            {
                _dbContext.AccountCountries.Remove(existingRow);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Upserts the device quantities for one country row and removes those no longer present.</summary>
    private static void ApplyDeviceCells(AccountCountry countryRow, IReadOnlyList<CountryDeviceValueDto> incomingDevices)
    {
        var existingByDevice = countryRow.DeviceCells.ToDictionary(cell => cell.DeviceRefId);
        var incomingDeviceIds = new HashSet<int>();

        foreach (var incomingDevice in incomingDevices)
        {
            incomingDeviceIds.Add(incomingDevice.DeviceRefId);

            if (existingByDevice.TryGetValue(incomingDevice.DeviceRefId, out var deviceCell))
            {
                deviceCell.Value = incomingDevice.Value;
            }
            else
            {
                countryRow.DeviceCells.Add(new CountryDeviceCell
                {
                    DeviceRefId = incomingDevice.DeviceRefId,
                    Value = incomingDevice.Value
                });
            }
        }

        var deviceCellsToRemove = countryRow.DeviceCells
            .Where(cell => !incomingDeviceIds.Contains(cell.DeviceRefId))
            .ToList();
        foreach (var deviceCell in deviceCellsToRemove)
        {
            countryRow.DeviceCells.Remove(deviceCell);
        }
    }
}
