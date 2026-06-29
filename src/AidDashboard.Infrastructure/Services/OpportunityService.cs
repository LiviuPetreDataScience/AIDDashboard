using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IOpportunityService"/>
public class OpportunityService : IOpportunityService
{
    private readonly AppDbContext _dbContext;

    public OpportunityService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OpportunityDto>> GetAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.Opportunities
            .AsNoTracking()
            .Where(opportunity => opportunity.AccountId == accountId)
            .OrderBy(opportunity => opportunity.Id)
            .ToListAsync(cancellationToken);

        return rows.Select(MapToDto).ToList();
    }

    public async Task<bool> SaveAsync(int accountId, IReadOnlyList<OpportunityDto> rows, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Accounts.AnyAsync(account => account.Id == accountId, cancellationToken))
        {
            return false;
        }

        var existingRows = await _dbContext.Opportunities
            .Where(opportunity => opportunity.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var existingById = existingRows.ToDictionary(opportunity => opportunity.Id);
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
                var created = new Opportunity { AccountId = accountId };
                ApplyFields(created, incoming);
                _dbContext.Opportunities.Add(created);
            }
        }

        foreach (var existing in existingRows)
        {
            if (!retainedIds.Contains(existing.Id))
            {
                _dbContext.Opportunities.Remove(existing);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ApplyFields(Opportunity entity, OpportunityDto dto)
    {
        entity.Name = dto.Name;
        entity.OpportunityTypeRefId = dto.OpportunityTypeRefId;
        entity.RelatedServiceRefId = dto.RelatedServiceRefId;
        entity.StatusRefId = dto.StatusRefId;
        entity.EstimatedMonthlyValue = dto.EstimatedMonthlyValue;
        entity.EstimatedContractDurationMonths = dto.EstimatedContractDurationMonths;
        entity.DeliveredByRefId = dto.DeliveredByRefId;
        entity.Details = dto.Details;
    }

    private static OpportunityDto MapToDto(Opportunity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        OpportunityTypeRefId = entity.OpportunityTypeRefId,
        RelatedServiceRefId = entity.RelatedServiceRefId,
        StatusRefId = entity.StatusRefId,
        EstimatedMonthlyValue = entity.EstimatedMonthlyValue,
        EstimatedContractDurationMonths = entity.EstimatedContractDurationMonths,
        DeliveredByRefId = entity.DeliveredByRefId,
        Details = entity.Details
    };
}
