using AidDashboard.Application.Dashboard;
using AidDashboard.Domain.Accounts;
using AidDashboard.Domain.Reference;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IDashboardService"/>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AutomationFactDto>> GetAutomationFactsAsync(CancellationToken cancellationToken = default)
    {
        // Reference id -> name, for the types used by the dashboard.
        var referenceNames = (await _dbContext.ReferenceItems
                .AsNoTracking()
                .Where(item => item.Type == ReferenceType.Automation
                    || item.Type == ReferenceType.AutomationCategory
                    || item.Type == ReferenceType.AutomationGoal
                    || item.Type == ReferenceType.Environment
                    || item.Type == ReferenceType.ServiceTower)
                .Select(item => new { item.Id, item.Name })
                .ToListAsync(cancellationToken))
            .ToDictionary(item => item.Id, item => item.Name);

        string? Name(int? id) => id is int value && referenceNames.TryGetValue(value, out var name) ? name : null;

        var profilesByAutomation = await _dbContext.AutomationProfiles
            .AsNoTracking()
            .ToDictionaryAsync(profile => profile.AutomationRefId, cancellationToken);

        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .Select(account => new { account.Id, account.Name, account.RegionalOwnership })
            .ToDictionaryAsync(account => account.Id, cancellationToken);

        var automations = await _dbContext.Automations.AsNoTracking().ToListAsync(cancellationToken);

        var facts = new List<AutomationFactDto>();
        foreach (var automation in automations)
        {
            accounts.TryGetValue(automation.AccountId, out var account);
            profilesByAutomation.TryGetValue(automation.AutomationRefId ?? 0, out var profile);

            facts.Add(new AutomationFactDto
            {
                AccountId = automation.AccountId,
                AccountName = account?.Name ?? string.Empty,
                RegionalOwnership = account?.RegionalOwnership,
                AutomationRefId = automation.AutomationRefId,
                AutomationName = Name(automation.AutomationRefId),
                Category = Name(profile?.CategoryRefId),
                Goal = Name(profile?.GoalRefId),
                Environment = Name(profile?.EnvironmentRefId),
                AiUsed = profile?.AiUsed,
                DeliveredBy = Name(automation.DeliveredByRefId),
                DeploymentDate = automation.DeploymentDate?.ToString("yyyy-MM-dd"),
                CostOfImplementation = automation.CostOfImplementationOneTime,
                RunningCost = automation.RunningCostMonthly,
                EfficiencyImpact = automation.EfficiencyImpactFtePerMonth,
            });
        }

        return facts;
    }

    public async Task<IReadOnlyList<AutomationProfileDto>> GetAutomationProfilesAsync(CancellationToken cancellationToken = default)
    {
        // Every automation in the reference list, whether or not it has a saved profile yet.
        var automations = await _dbContext.ReferenceItems
            .AsNoTracking()
            .Where(item => item.Type == ReferenceType.Automation)
            .OrderBy(item => item.Name)
            .Select(item => new { item.Id, item.Name })
            .ToListAsync(cancellationToken);

        var profilesByAutomation = await _dbContext.AutomationProfiles
            .AsNoTracking()
            .ToDictionaryAsync(profile => profile.AutomationRefId, cancellationToken);

        return automations.Select(automation =>
        {
            profilesByAutomation.TryGetValue(automation.Id, out var profile);
            return new AutomationProfileDto
            {
                AutomationRefId = automation.Id,
                AutomationName = automation.Name,
                CategoryRefId = profile?.CategoryRefId,
                GoalRefId = profile?.GoalRefId,
                EnvironmentRefId = profile?.EnvironmentRefId,
                AiUsed = profile?.AiUsed,
            };
        }).ToList();
    }

    public async Task<bool> SaveAutomationProfileAsync(int automationRefId, UpdateAutomationProfileRequest request, CancellationToken cancellationToken = default)
    {
        // Guard against editing an automation that no longer exists in the reference list.
        var automationExists = await _dbContext.ReferenceItems
            .AnyAsync(item => item.Id == automationRefId && item.Type == ReferenceType.Automation, cancellationToken);
        if (!automationExists)
        {
            return false;
        }

        var profile = await _dbContext.AutomationProfiles
            .FirstOrDefaultAsync(p => p.AutomationRefId == automationRefId, cancellationToken);
        if (profile is null)
        {
            profile = new AutomationProfile { AutomationRefId = automationRefId };
            _dbContext.AutomationProfiles.Add(profile);
        }

        profile.CategoryRefId = request.CategoryRefId;
        profile.GoalRefId = request.GoalRefId;
        profile.EnvironmentRefId = request.EnvironmentRefId;
        profile.AiUsed = request.AiUsed;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
