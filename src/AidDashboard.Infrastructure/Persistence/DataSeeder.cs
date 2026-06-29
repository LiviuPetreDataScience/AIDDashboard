using AidDashboard.Application.Abstractions;
using AidDashboard.Domain.Identity;
using AidDashboard.Domain.Reference;
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

        await db.SaveChangesAsync(ct);
    }
}
