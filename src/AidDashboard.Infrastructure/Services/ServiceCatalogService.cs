using AidDashboard.Application.Services;
using AidDashboard.Domain.Services;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IServiceCatalogService"/>
public class ServiceCatalogService : IServiceCatalogService
{
    private readonly AppDbContext _dbContext;

    public ServiceCatalogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ServiceCatalogItemDto>> GetChildrenAsync(int? parentId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ServiceCatalogItems.AsNoTracking().Where(item => item.ParentId == parentId);
        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }

        var items = await query.OrderBy(item => item.SortOrder).ThenBy(item => item.Name).ToListAsync(cancellationToken);
        var ids = items.Select(item => item.Id).ToList();

        // Which of these items have at least one child (so the UI can show a "drill in" affordance).
        var parentsWithChildren = (await _dbContext.ServiceCatalogItems.AsNoTracking()
                .Where(child => child.ParentId != null && ids.Contains(child.ParentId!.Value))
                .Select(child => child.ParentId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();

        return items
            .Select(item => new ServiceCatalogItemDto(
                item.Id, item.ParentId, item.Name, item.Kind, item.SortOrder, item.IsActive,
                parentsWithChildren.Contains(item.Id)))
            .ToList();
    }

    public async Task<IReadOnlyList<BreadcrumbItemDto>> GetBreadcrumbAsync(int itemId, CancellationToken cancellationToken = default)
    {
        var byId = await LoadAllByIdAsync(cancellationToken);
        var breadcrumb = new List<BreadcrumbItemDto>();

        int? currentId = itemId;
        while (currentId != null && byId.TryGetValue(currentId.Value, out var current))
        {
            breadcrumb.Insert(0, new BreadcrumbItemDto(current.Id, current.Name));
            currentId = current.ParentId;
        }

        return breadcrumb;
    }

    public async Task<IReadOnlyList<ServiceLeafDto>> GetLeafServicesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ServiceCatalogItems.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }
        var all = await query.ToListAsync(cancellationToken);

        // Group children by parent, using 0 as the sentinel key for the top-level (null parent).
        const int rootKey = 0;
        var childrenByParent = new Dictionary<int, List<ServiceCatalogItem>>();
        foreach (var group in all.GroupBy(item => item.ParentId ?? rootKey))
        {
            childrenByParent[group.Key] = group.OrderBy(i => i.SortOrder).ThenBy(i => i.Name).ToList();
        }

        // Depth-first walk in catalog order, accumulating the ancestor category names as the path.
        var leaves = new List<ServiceLeafDto>();

        void Visit(ServiceCatalogItem node, IReadOnlyList<string> ancestorNames)
        {
            if (node.Kind == ServiceNodeKind.Service)
            {
                leaves.Add(new ServiceLeafDto(node.Id, node.Name, string.Join(" -> ", ancestorNames)));
                return;
            }
            if (childrenByParent.TryGetValue(node.Id, out var children))
            {
                var nextAncestors = ancestorNames.Append(node.Name).ToList();
                foreach (var child in children)
                {
                    Visit(child, nextAncestors);
                }
            }
        }

        if (childrenByParent.TryGetValue(rootKey, out var roots))
        {
            foreach (var root in roots)
            {
                Visit(root, Array.Empty<string>());
            }
        }

        return leaves;
    }

    public async Task<ServiceCatalogItemDto> CreateAsync(CreateServiceItemRequest request, CancellationToken cancellationToken = default)
    {
        var nextSortOrder = (await _dbContext.ServiceCatalogItems
            .Where(item => item.ParentId == request.ParentId)
            .Select(item => (int?)item.SortOrder)
            .MaxAsync(cancellationToken) ?? -1) + 1;

        var entity = new ServiceCatalogItem
        {
            ParentId = request.ParentId,
            Name = request.Name.Trim(),
            Kind = request.Kind,
            SortOrder = nextSortOrder,
            IsActive = true
        };
        _dbContext.ServiceCatalogItems.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ServiceCatalogItemDto(entity.Id, entity.ParentId, entity.Name, entity.Kind, entity.SortOrder, entity.IsActive, false);
    }

    public async Task<bool> UpdateAsync(int id, UpdateServiceItemRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ServiceCatalogItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
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
        var entity = await _dbContext.ServiceCatalogItems.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        // Cascade delete removes the whole subtree.
        _dbContext.ServiceCatalogItems.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Dictionary<int, ServiceCatalogItem>> LoadAllByIdAsync(CancellationToken cancellationToken) =>
        await _dbContext.ServiceCatalogItems.AsNoTracking().ToDictionaryAsync(item => item.Id, cancellationToken);
}
