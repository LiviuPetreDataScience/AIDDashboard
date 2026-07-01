using AidDashboard.Application.Abstractions;
using AidDashboard.Domain.Accounts;
using AidDashboard.Domain.Identity;
using AidDashboard.Domain.Reference;
using AidDashboard.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Persistence;

/// <summary>
/// Idempotent seeding: ensures reference lists are populated and the five placeholder
/// admin accounts exist. Safe to run on every startup.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher, CancellationToken ct = default)
    {
        foreach (var (type, names) in ReferenceData.Items)
        {
            var existing = (await db.ReferenceItems
                    .Where(r => r.Type == type)
                    .Select(r => r.Name)
                    .ToListAsync(ct))
                .ToHashSet();

            for (var order = 0; order < names.Length; order++)
            {
                if (!existing.Contains(names[order]))
                {
                    db.ReferenceItems.Add(new ReferenceItem
                    {
                        Type = type,
                        Name = names[order],
                        SortOrder = order,
                        IsActive = true
                    });
                }
            }
        }

        // Placeholder admins admin1..admin5 — temporary password equals the username (per agreed setup).
        if (!await db.Users.AnyAsync(ct))
        {
            for (var i = 1; i <= 5; i++)
            {
                var name = $"admin{i}";
                db.Users.Add(new User
                {
                    Username = name,
                    DisplayName = $"Admin {i}",
                    PasswordHash = hasher.Hash(name),
                    Role = UserRole.Admin,
                    IsActive = true,
                    Provider = "Local"
                });
            }
        }

        // Services hierarchy: seed once if the catalog is empty.
        if (!await db.ServiceCatalogItems.AnyAsync(ct))
        {
            foreach (var (node, order) in ServicesSeedData.Categories.Select((node, index) => (node, index)))
            {
                AddServiceNode(db, node, parentId: null, sortOrder: order);
            }
        }

        // Persist reference items so their ids are available for the automation profiles below.
        await db.SaveChangesAsync(ct);
        await SeedAutomationProfilesAsync(db, ct);

        await db.SaveChangesAsync(ct);
    }

    /// <summary>Creates one attributes profile per automation (once), populated from the sample data.</summary>
    private static async Task SeedAutomationProfilesAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.AutomationProfiles.AnyAsync(ct))
        {
            return;
        }

        var relevant = await db.ReferenceItems
            .Where(item => item.Type == ReferenceType.Automation
                || item.Type == ReferenceType.AutomationCategory
                || item.Type == ReferenceType.AutomationGoal
                || item.Type == ReferenceType.Environment)
            .ToListAsync(ct);

        int? LookupId(ReferenceType type, string? name) =>
            name is null ? null : relevant.FirstOrDefault(r => r.Type == type && r.Name == name)?.Id;

        foreach (var automation in relevant.Where(r => r.Type == ReferenceType.Automation))
        {
            AutomationsSeedData.Attributes.TryGetValue(automation.Name, out var attributes);
            db.AutomationProfiles.Add(new AutomationProfile
            {
                AutomationRefId = automation.Id,
                CategoryRefId = LookupId(ReferenceType.AutomationCategory, attributes?.Category),
                GoalRefId = LookupId(ReferenceType.AutomationGoal, attributes?.Goal),
                EnvironmentRefId = LookupId(ReferenceType.Environment, attributes?.Environment),
                AiUsed = attributes?.AiUsed,
            });
        }
    }

    /// <summary>Recursively inserts a seed node and its children. Leaves (no children) are services.</summary>
    private static void AddServiceNode(AppDbContext db, ServiceSeedNode node, int? parentId, int sortOrder)
    {
        var entity = new ServiceCatalogItem
        {
            ParentId = parentId,
            Name = node.Name,
            Kind = node.Children.Count > 0 ? ServiceNodeKind.Category : ServiceNodeKind.Service,
            SortOrder = sortOrder,
            IsActive = true
        };
        db.ServiceCatalogItems.Add(entity);

        if (node.Children.Count == 0)
        {
            return;
        }

        // The entity needs an id before its children can reference it as parent.
        db.SaveChanges();
        for (var index = 0; index < node.Children.Count; index++)
        {
            AddServiceNode(db, node.Children[index], entity.Id, index);
        }
    }
}
