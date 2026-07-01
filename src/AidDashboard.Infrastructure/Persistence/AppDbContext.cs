using System.Text.Json;
using AidDashboard.Application.Abstractions;
using AidDashboard.Domain.Accounts;
using AidDashboard.Domain.Common;
using AidDashboard.Domain.Identity;
using AidDashboard.Domain.Reference;
using AidDashboard.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AidDashboard.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserAccessor? _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserAccessor? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ReferenceItem> ReferenceItems => Set<ReferenceItem>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AccountMultiSelectValue> AccountMultiSelectValues => Set<AccountMultiSelectValue>();
    public DbSet<StaffingCell> StaffingCells => Set<StaffingCell>();
    public DbSet<ContractualLanguageCell> ContractualLanguageCells => Set<ContractualLanguageCell>();
    public DbSet<AccountCountry> AccountCountries => Set<AccountCountry>();
    public DbSet<CountryDeviceCell> CountryDeviceCells => Set<CountryDeviceCell>();
    public DbSet<Automation> Automations => Set<Automation>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<SlaKpi> SlaKpis => Set<SlaKpi>();
    public DbSet<SupportHour> SupportHours => Set<SupportHour>();
    public DbSet<ServiceCatalogItem> ServiceCatalogItems => Set<ServiceCatalogItem>();
    public DbSet<AccountServiceEntry> AccountServiceEntries => Set<AccountServiceEntry>();
    public DbSet<AutomationProfile> AutomationProfiles => Set<AutomationProfile>();
    public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(256).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.DisplayName).HasMaxLength(256);
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.Provider).HasMaxLength(50);
        });

        b.Entity<ReferenceItem>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasIndex(x => new { x.Type, x.Name }).IsUnique();
            e.HasIndex(x => x.Type);
        });

        b.Entity<Account>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();

            e.HasMany(x => x.MultiSelectValues).WithOne().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.StaffingCells).WithOne().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.ContractualLanguageCells).WithOne().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Countries).WithOne().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Automations).WithOne().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Opportunities).WithOne().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.SlaKpis).WithOne().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.SupportHours).WithOne().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<AccountMultiSelectValue>(e =>
            e.HasIndex(x => new { x.AccountId, x.Field, x.ReferenceItemId }).IsUnique());

        b.Entity<StaffingCell>(e =>
            e.HasIndex(x => new { x.AccountId, x.ModelType, x.LocationRefId, x.RoleRefId }).IsUnique());

        b.Entity<ContractualLanguageCell>(e =>
            e.HasIndex(x => new { x.AccountId, x.LocationRefId, x.LanguageRefId }).IsUnique());

        b.Entity<AccountCountry>(e =>
        {
            e.HasIndex(x => new { x.AccountId, x.CountryRefId }).IsUnique();
            e.HasMany(x => x.DeviceCells).WithOne().HasForeignKey(x => x.AccountCountryId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<CountryDeviceCell>(e =>
            e.HasIndex(x => new { x.AccountCountryId, x.DeviceRefId }).IsUnique());

        b.Entity<SupportHour>(e =>
            e.HasIndex(x => new { x.AccountId, x.LanguageRefId }).IsUnique());

        b.Entity<ServiceCatalogItem>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(300).IsRequired();
            e.HasIndex(x => x.ParentId);
            // Deleting a category removes its whole subtree.
            e.HasMany(x => x.Children).WithOne().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<AccountServiceEntry>(e =>
        {
            e.HasIndex(x => new { x.AccountId, x.ServiceItemId }).IsUnique();
            e.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<AutomationProfile>(e =>
            e.HasIndex(x => x.AutomationRefId).IsUnique());

        b.Entity<Automation>().Property(x => x.CostOfImplementationOneTime).HasColumnType("TEXT");
        b.Entity<Automation>().Property(x => x.RunningCostMonthly).HasColumnType("TEXT");
        b.Entity<Opportunity>().Property(x => x.EstimatedMonthlyValue).HasColumnType("TEXT");

        b.Entity<ChangeLog>(e =>
        {
            e.Property(x => x.EntityType).HasMaxLength(128);
            e.Property(x => x.Action).HasMaxLength(16);
            e.HasIndex(x => x.AccountId);
            e.HasIndex(x => new { x.EntityType, x.EntityId });
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var pending = ApplyAuditAndCapture();
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        if (FinalizeChangeLogs(pending))
            base.SaveChanges(acceptAllChangesOnSuccess);
        return result;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        var pending = ApplyAuditAndCapture();
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
        if (FinalizeChangeLogs(pending))
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
        return result;
    }

    /// <summary>Captured change metadata, resolved into a ChangeLog after the entities are saved.</summary>
    private sealed record PendingChange(EntityEntry Entry, string Action, string? ChangesJson, string? IdPreSave, bool IsAdded);

    private List<PendingChange> ApplyAuditAndCapture()
    {
        var now = DateTime.UtcNow;
        var user = _currentUser?.UserName;
        var pending = new List<PendingChange>();

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            // Audit stamping and change history apply only to record-level entities
            // (those deriving from AuditableEntity). High-volume matrix cells and multi-select
            // join rows are persisted but not individually logged, to keep the history meaningful.
            if (entry.Entity is not AuditableEntity auditable) continue;

            if (entry.State == EntityState.Added)
            {
                auditable.CreatedAtUtc = now;
                auditable.CreatedBy = user;
            }
            else if (entry.State == EntityState.Modified)
            {
                auditable.ModifiedAtUtc = now;
                auditable.ModifiedBy = user;
                entry.Property(nameof(AuditableEntity.CreatedAtUtc)).IsModified = false;
                entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
            }

            var isAdded = entry.State == EntityState.Added;
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Deleted => "Deleted",
                _ => "Updated"
            };

            string? changesJson = null;
            if (entry.State == EntityState.Modified)
            {
                var changes = new Dictionary<string, object?>();
                foreach (var p in entry.Properties)
                {
                    if (!p.IsModified) continue;
                    if (p.Metadata.Name is nameof(AuditableEntity.ModifiedAtUtc) or nameof(AuditableEntity.ModifiedBy)) continue;
                    changes[p.Metadata.Name] = new { from = p.OriginalValue, to = p.CurrentValue };
                }
                if (changes.Count > 0)
                    changesJson = JsonSerializer.Serialize(changes);
            }

            // For modified/deleted the key is already valid; for added it is resolved post-save.
            var idPreSave = isAdded ? null : GetId(entry);
            pending.Add(new PendingChange(entry, action, changesJson, idPreSave, isAdded));
        }

        return pending;
    }

    /// <summary>Returns true if change-log rows were queued and a follow-up save is required.</summary>
    private bool FinalizeChangeLogs(List<PendingChange> pending)
    {
        if (pending.Count == 0) return false;
        var now = DateTime.UtcNow;
        var user = _currentUser?.UserName;

        foreach (var p in pending)
        {
            ChangeLogs.Add(new ChangeLog
            {
                EntityType = p.Entry.Entity.GetType().Name,
                EntityId = p.IsAdded ? GetId(p.Entry) : (p.IdPreSave ?? string.Empty),
                AccountId = GetAccountId(p.Entry),
                Action = p.Action,
                ChangesJson = p.ChangesJson,
                ChangedBy = user,
                ChangedAtUtc = now
            });
        }
        return true;
    }

    private static string GetId(EntityEntry entry)
    {
        try
        {
            var key = entry.Metadata.FindPrimaryKey();
            if (key is null) return string.Empty;
            return string.Join(",", key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString()));
        }
        catch { return string.Empty; }
    }

    private static int? GetAccountId(EntityEntry entry)
    {
        try
        {
            if (entry.Entity is Account a) return a.Id;
            if (entry.Metadata.FindProperty("AccountId") is not null && entry.Property("AccountId").CurrentValue is int i)
                return i;
        }
        catch { /* entry may be detached after a delete */ }
        return null;
    }
}
