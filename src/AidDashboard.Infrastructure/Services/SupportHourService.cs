using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="ISupportHourService"/>
public class SupportHourService : ISupportHourService
{
    private readonly AppDbContext _dbContext;

    public SupportHourService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SupportHourRowDto>> GetAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.SupportHours
            .AsNoTracking()
            .Where(row => row.AccountId == accountId)
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new SupportHourRowDto(
                row.LanguageRefId,
                row.FromMondayFriday,
                row.ToMondayFriday,
                row.Coverage,
                row.OnCallInterpretDhs,
                row.Sophie))
            .ToList();
    }

    public async Task<bool> SaveAsync(int accountId, IReadOnlyList<SupportHourRowDto> rows, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Accounts.AnyAsync(account => account.Id == accountId, cancellationToken))
        {
            return false;
        }

        var existingRows = await _dbContext.SupportHours
            .Where(row => row.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var existingByLanguage = existingRows.ToDictionary(row => row.LanguageRefId);
        var incomingLanguageIds = new HashSet<int>();

        foreach (var incoming in rows)
        {
            incomingLanguageIds.Add(incoming.LanguageRefId);

            if (!existingByLanguage.TryGetValue(incoming.LanguageRefId, out var row))
            {
                row = new SupportHour
                {
                    AccountId = accountId,
                    LanguageRefId = incoming.LanguageRefId
                };
                _dbContext.SupportHours.Add(row);
            }

            row.FromMondayFriday = incoming.FromMondayFriday;
            row.ToMondayFriday = incoming.ToMondayFriday;
            row.Coverage = incoming.Coverage;
            row.OnCallInterpretDhs = incoming.OnCallInterpretDhs;
            row.Sophie = incoming.Sophie;
        }

        foreach (var existingRow in existingRows)
        {
            if (!incomingLanguageIds.Contains(existingRow.LanguageRefId))
            {
                _dbContext.SupportHours.Remove(existingRow);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
