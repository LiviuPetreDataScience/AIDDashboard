using AidDashboard.Application.Services;
using AidDashboard.Domain.Services;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IAccountServicesService"/>
public class AccountServicesService : IAccountServicesService
{
    private readonly AppDbContext _dbContext;
    private readonly IServiceCatalogService _catalog;

    public AccountServicesService(AppDbContext dbContext, IServiceCatalogService catalog)
    {
        _dbContext = dbContext;
        _catalog = catalog;
    }

    public async Task<IReadOnlyList<AccountServiceRowDto>> GetAsync(int accountId, CancellationToken cancellationToken = default)
    {
        // Rows are the full set of active leaf services (in catalog order), merged with saved data.
        var leaves = await _catalog.GetLeafServicesAsync(false, cancellationToken);

        var savedByService = await _dbContext.AccountServiceEntries
            .AsNoTracking()
            .Where(entry => entry.AccountId == accountId)
            .ToDictionaryAsync(entry => entry.ServiceItemId, cancellationToken);

        return leaves
            .Select(leaf =>
            {
                savedByService.TryGetValue(leaf.Id, out var entry);
                return new AccountServiceRowDto
                {
                    ServiceItemId = leaf.Id,
                    Path = leaf.Path,
                    ServiceName = leaf.ServiceName,
                    ContractEndDate = entry?.ContractEndDate,
                    ExistsInCompany = entry?.ExistsInCompany,
                    ProvidedByStefaniniGroup = entry?.ProvidedByStefaniniGroup,
                    ProvidedByRefId = entry?.ProvidedByRefId,
                    OpportunityToProvide = entry?.OpportunityToProvide,
                    Details = entry?.Details
                };
            })
            .ToList();
    }

    public async Task<bool> SaveAsync(int accountId, IReadOnlyList<AccountServiceRowDto> rows, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Accounts.AnyAsync(account => account.Id == accountId, cancellationToken))
        {
            return false;
        }

        var existingEntries = await _dbContext.AccountServiceEntries
            .Where(entry => entry.AccountId == accountId)
            .ToListAsync(cancellationToken);
        var existingByService = existingEntries.ToDictionary(entry => entry.ServiceItemId);

        foreach (var row in rows)
        {
            var hasData = row.ContractEndDate.HasValue
                || row.ExistsInCompany.HasValue
                || row.ProvidedByStefaniniGroup.HasValue
                || row.ProvidedByRefId.HasValue
                || row.OpportunityToProvide.HasValue
                || !string.IsNullOrWhiteSpace(row.Details);

            existingByService.TryGetValue(row.ServiceItemId, out var entry);

            if (!hasData)
            {
                // Nothing to store: drop any previously-saved entry for this service.
                if (entry is not null)
                {
                    _dbContext.AccountServiceEntries.Remove(entry);
                }
                continue;
            }

            if (entry is null)
            {
                entry = new AccountServiceEntry { AccountId = accountId, ServiceItemId = row.ServiceItemId };
                _dbContext.AccountServiceEntries.Add(entry);
            }

            entry.ContractEndDate = row.ContractEndDate;
            entry.ExistsInCompany = row.ExistsInCompany;
            entry.ProvidedByStefaniniGroup = row.ProvidedByStefaniniGroup;
            entry.ProvidedByRefId = row.ProvidedByRefId;
            entry.OpportunityToProvide = row.OpportunityToProvide;
            entry.Details = row.Details;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
