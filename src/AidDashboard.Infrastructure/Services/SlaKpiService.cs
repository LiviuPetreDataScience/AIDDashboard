using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="ISlaKpiService"/>
public class SlaKpiService : ISlaKpiService
{
    private readonly AppDbContext _dbContext;

    public SlaKpiService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SlaKpiDto>> GetAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.SlaKpis
            .AsNoTracking()
            .Where(slaKpi => slaKpi.AccountId == accountId)
            .OrderBy(slaKpi => slaKpi.Id)
            .ToListAsync(cancellationToken);

        return rows.Select(MapToDto).ToList();
    }

    public async Task<bool> SaveAsync(int accountId, IReadOnlyList<SlaKpiDto> rows, CancellationToken cancellationToken = default)
    {
        if (!await _dbContext.Accounts.AnyAsync(account => account.Id == accountId, cancellationToken))
        {
            return false;
        }

        var existingRows = await _dbContext.SlaKpis
            .Where(slaKpi => slaKpi.AccountId == accountId)
            .ToListAsync(cancellationToken);

        var existingById = existingRows.ToDictionary(slaKpi => slaKpi.Id);
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
                var created = new SlaKpi { AccountId = accountId };
                ApplyFields(created, incoming);
                _dbContext.SlaKpis.Add(created);
            }
        }

        foreach (var existing in existingRows)
        {
            if (!retainedIds.Contains(existing.Id))
            {
                _dbContext.SlaKpis.Remove(existing);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ApplyFields(SlaKpi entity, SlaKpiDto dto)
    {
        entity.Name = dto.Name;
        entity.TypeRefId = dto.TypeRefId;
        entity.Description = dto.Description;
        entity.Formula = dto.Formula;
        entity.TargetPercent = dto.TargetPercent;
        entity.MeasurementTypeRefId = dto.MeasurementTypeRefId;
        entity.CanBeReported = dto.CanBeReported;
        entity.FinancialPenalties = dto.FinancialPenalties;
        entity.Bonus = dto.Bonus;
    }

    private static SlaKpiDto MapToDto(SlaKpi entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        TypeRefId = entity.TypeRefId,
        Description = entity.Description,
        Formula = entity.Formula,
        TargetPercent = entity.TargetPercent,
        MeasurementTypeRefId = entity.MeasurementTypeRefId,
        CanBeReported = entity.CanBeReported,
        FinancialPenalties = entity.FinancialPenalties,
        Bonus = entity.Bonus
    };
}
