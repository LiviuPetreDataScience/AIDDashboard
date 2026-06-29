using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IContractualLanguageService"/>
public class ContractualLanguageService : IContractualLanguageService
{
    private readonly AppDbContext _dbContext;

    public ContractualLanguageService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ContractualLanguageCellDto>> GetAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContractualLanguageCells
            .AsNoTracking()
            .Where(cell => cell.AccountId == accountId)
            .Select(cell => new ContractualLanguageCellDto(cell.LocationRefId, cell.LanguageRefId, cell.Value))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SaveAsync(int accountId, IReadOnlyList<ContractualLanguageCellDto> cells, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Accounts.AnyAsync(account => account.Id == accountId, cancellationToken))
        {
            return false;
        }

        var existingCells = await _dbContext.ContractualLanguageCells
            .Where(cell => cell.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var existingByLocationAndLanguage = existingCells
            .ToDictionary(cell => (cell.LocationRefId, cell.LanguageRefId));
        var incomingKeys = new HashSet<(int LocationRefId, int LanguageRefId)>();

        foreach (var incoming in cells)
        {
            var key = (incoming.LocationRefId, incoming.LanguageRefId);
            incomingKeys.Add(key);

            if (existingByLocationAndLanguage.TryGetValue(key, out var existingCell))
            {
                existingCell.Value = incoming.Value;
            }
            else
            {
                _dbContext.ContractualLanguageCells.Add(new ContractualLanguageCell
                {
                    AccountId = accountId,
                    LocationRefId = incoming.LocationRefId,
                    LanguageRefId = incoming.LanguageRefId,
                    Value = incoming.Value
                });
            }
        }

        foreach (var existingCell in existingCells)
        {
            if (!incomingKeys.Contains((existingCell.LocationRefId, existingCell.LanguageRefId)))
            {
                _dbContext.ContractualLanguageCells.Remove(existingCell);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
