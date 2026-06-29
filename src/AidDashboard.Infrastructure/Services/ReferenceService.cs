using AidDashboard.Application.Reference;
using AidDashboard.Domain.Reference;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IReferenceService"/>
public class ReferenceService : IReferenceService
{
    private readonly AppDbContext _dbContext;

    public ReferenceService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReferenceItemDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ReferenceItems.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }

        var items = await query
            .OrderBy(item => item.Type)
            .ThenBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ReferenceItemDto>> GetByTypeAsync(ReferenceType type, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ReferenceItems.AsNoTracking().Where(item => item.Type == type);
        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }

        var items = await query
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Name)
            .ToListAsync(cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    public async Task<ReferenceItemDto> CreateAsync(CreateReferenceItemRequest request, CancellationToken cancellationToken = default)
    {
        // Default the sort order to the end of the list when one is not supplied.
        var sortOrder = request.SortOrder ?? await NextSortOrderAsync(request.Type, cancellationToken);

        var entity = new ReferenceItem
        {
            Type = request.Type,
            Name = request.Name.Trim(),
            SortOrder = sortOrder,
            IsActive = true
        };

        _dbContext.ReferenceItems.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, UpdateReferenceItemRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ReferenceItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.Name = request.Name.Trim();
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ReferenceItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        _dbContext.ReferenceItems.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int> NextSortOrderAsync(ReferenceType type, CancellationToken cancellationToken)
    {
        var maxSortOrder = await _dbContext.ReferenceItems
            .Where(item => item.Type == type)
            .Select(item => (int?)item.SortOrder)
            .MaxAsync(cancellationToken);

        return (maxSortOrder ?? -1) + 1;
    }

    private static ReferenceItemDto MapToDto(ReferenceItem entity) =>
        new(entity.Id, entity.Type, entity.Name, entity.SortOrder, entity.IsActive);
}
