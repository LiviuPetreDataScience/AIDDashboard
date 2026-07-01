using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IAutomationService"/>
public class AutomationService : IAutomationService
{
    private readonly AppDbContext _dbContext;

    public AutomationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AutomationDto>> GetAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.Automations
            .AsNoTracking()
            .Where(automation => automation.AccountId == accountId)
            .OrderBy(automation => automation.Id)
            .ToListAsync(cancellationToken);

        return rows.Select(MapToDto).ToList();
    }

    public async Task<bool> SaveAsync(int accountId, IReadOnlyList<AutomationDto> rows, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Accounts.AnyAsync(account => account.Id == accountId, cancellationToken))
        {
            return false;
        }

        var existingRows = await _dbContext.Automations
            .Where(automation => automation.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var existingById = existingRows.ToDictionary(automation => automation.Id);
        var retainedIds = new HashSet<int>();

        foreach (var incoming in rows)
        {
            if (incoming.Id > 0 && existingById.TryGetValue(incoming.Id, out var existing))
            {
                ApplyFields(existing, incoming);
                retainedIds.Add(existing.Id);
            }
            else
            {
                var created = new Automation { AccountId = accountId };
                ApplyFields(created, incoming);
                _dbContext.Automations.Add(created);
            }
        }

        foreach (var existing in existingRows)
        {
            if (!retainedIds.Contains(existing.Id))
            {
                _dbContext.Automations.Remove(existing);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ApplyFields(Automation entity, AutomationDto dto)
    {
        entity.AutomationRefId = dto.AutomationRefId;
        entity.DeploymentDate = dto.DeploymentDate;
        entity.CostOfImplementationOneTime = dto.CostOfImplementationOneTime;
        entity.RunningCostMonthly = dto.RunningCostMonthly;
        entity.EfficiencyImpactFtePerMonth = dto.EfficiencyImpactFtePerMonth;
        entity.DeliveredByRefId = dto.DeliveredByRefId;
        entity.Details = dto.Details;
    }

    private static AutomationDto MapToDto(Automation entity) => new()
    {
        Id = entity.Id,
        AutomationRefId = entity.AutomationRefId,
        DeploymentDate = entity.DeploymentDate,
        CostOfImplementationOneTime = entity.CostOfImplementationOneTime,
        RunningCostMonthly = entity.RunningCostMonthly,
        EfficiencyImpactFtePerMonth = entity.EfficiencyImpactFtePerMonth,
        DeliveredByRefId = entity.DeliveredByRefId,
        Details = entity.Details
    };
}
