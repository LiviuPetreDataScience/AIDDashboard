using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IStaffingService"/>
public class StaffingService : IStaffingService
{
    private readonly AppDbContext _dbContext;

    public StaffingService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<StaffingCellDto>> GetAsync(int accountId, StaffingModelType modelType, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StaffingCells
            .AsNoTracking()
            .Where(cell => cell.AccountId == accountId && cell.ModelType == modelType)
            .Select(cell => new StaffingCellDto(cell.LocationRefId, cell.RoleRefId, cell.Value))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SaveAsync(int accountId, StaffingModelType modelType, IReadOnlyList<StaffingCellDto> cells, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Accounts.AnyAsync(account => account.Id == accountId, cancellationToken))
        {
            return false;
        }

        var existingCells = await _dbContext.StaffingCells
            .Where(cell => cell.AccountId == accountId && cell.ModelType == modelType)
            .ToListAsync(cancellationToken);

        var existingByLocationAndRole = existingCells
            .ToDictionary(cell => (cell.LocationRefId, cell.RoleRefId));
        var incomingKeys = new HashSet<(int LocationRefId, int RoleRefId)>();

        foreach (var incoming in cells)
        {
            var key = (incoming.LocationRefId, incoming.RoleRefId);
            incomingKeys.Add(key);

            if (existingByLocationAndRole.TryGetValue(key, out var existingCell))
            {
                existingCell.Value = incoming.Value;
            }
            else
            {
                _dbContext.StaffingCells.Add(new StaffingCell
                {
                    AccountId = accountId,
                    ModelType = modelType,
                    LocationRefId = incoming.LocationRefId,
                    RoleRefId = incoming.RoleRefId,
                    Value = incoming.Value
                });
            }
        }

        foreach (var existingCell in existingCells)
        {
            if (!incomingKeys.Contains((existingCell.LocationRefId, existingCell.RoleRefId)))
            {
                _dbContext.StaffingCells.Remove(existingCell);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
