using AidDashboard.Application.Accounts;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AidDashboard.Infrastructure.Services;

/// <inheritdoc cref="IAccountService"/>
public class AccountService : IAccountService
{
    private readonly AppDbContext _dbContext;

    public AccountService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccountSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .Select(account => new { account.Id, account.Name, account.CreatedAtUtc, account.ModifiedAtUtc })
            .ToListAsync(cancellationToken);

        // The latest recorded change per account, across all its tabs (from the change history).
        var latestChangePerAccount = (await _dbContext.ChangeLogs
                .AsNoTracking()
                .Where(log => log.AccountId != null)
                .GroupBy(log => log.AccountId!.Value)
                .Select(group => new { AccountId = group.Key, Latest = group.Max(log => log.ChangedAtUtc) })
                .ToListAsync(cancellationToken))
            .ToDictionary(entry => entry.AccountId, entry => entry.Latest);

        return accounts
            .Select(account =>
            {
                var lastUpdated = account.ModifiedAtUtc ?? account.CreatedAtUtc;
                if (latestChangePerAccount.TryGetValue(account.Id, out var latestChange) && latestChange > lastUpdated)
                {
                    lastUpdated = latestChange;
                }
                return new AccountSummaryDto(account.Id, account.Name, lastUpdated);
            })
            .OrderBy(account => account.Name)
            .ToList();
    }

    public async Task<AccountSummaryDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        var account = new Account { Name = request.Name.Trim() };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new AccountSummaryDto(account.Id, account.Name, account.CreatedAtUtc);
    }

    public async Task<AccountOverallDto?> GetOverallAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.Accounts
            .AsNoTracking()
            .Include(a => a.MultiSelectValues)
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        return account is null ? null : MapToOverallDto(account);
    }

    public async Task<bool> UpdateOverallAsync(int accountId, AccountOverallDto overall, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.Accounts
            .Include(a => a.MultiSelectValues)
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);

        if (account is null)
        {
            return false;
        }

        ApplyScalarFields(account, overall);
        ApplyMultiSelectFields(account, overall);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        if (account is null)
        {
            return false;
        }

        _dbContext.Accounts.Remove(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>Copies the scalar (non-collection) fields from the DTO onto the entity, excluding the name/id.</summary>
    private static void ApplyScalarFields(Account account, AccountOverallDto overall)
    {
        account.RegionalOwnership = overall.RegionalOwnership;
        account.GlobalSm = overall.GlobalSm;
        account.GlobalSdm = overall.GlobalSdm;
        account.EmeaLtsSdm = overall.EmeaLtsSdm;
        account.NaLtsSdm = overall.NaLtsSdm;
        account.EmeaSm = overall.EmeaSm;
        account.EmeaSdm = overall.EmeaSdm;
        account.NaSdm = overall.NaSdm;
        account.ApacSdm = overall.ApacSdm;
        account.Tsm = overall.Tsm;
        account.BuLeader = overall.BuLeader;
        account.TransformationManager = overall.TransformationManager;
        account.AccountManagerCrd = overall.AccountManagerCrd;
        account.LaunchDate = overall.LaunchDate;
        account.ContractEndDate = overall.ContractEndDate;

        account.AccountTypeRefId = overall.AccountTypeRefId;
        account.FlagshipAccount = overall.FlagshipAccount;

        account.IndustryRefId = overall.IndustryRefId;
        account.HeadquarterCountryRefId = overall.HeadquarterCountryRefId;
        account.ContractSupervisor = overall.ContractSupervisor;
        account.NoOfUsersSupported = overall.NoOfUsersSupported;
        account.SharingConstraints = overall.SharingConstraints;

        account.ConnectivityRefId = overall.ConnectivityRefId;
        account.TelecomCountry = overall.TelecomCountry;
        account.TelecomRefId = overall.TelecomRefId;
        account.ClientHardware = overall.ClientHardware;
        account.ItsmToolRefId = overall.ItsmToolRefId;
        account.ManagedByRefId = overall.ManagedByRefId;

        account.ProjectTrainingDurationDays = overall.ProjectTrainingDurationDays;
        account.SupportChannels = overall.SupportChannels;
        account.WorkFromHomeApproved = overall.WorkFromHomeApproved;
    }

    /// <summary>Replaces the account's multi-select selections with the values from the DTO.</summary>
    private void ApplyMultiSelectFields(Account account, AccountOverallDto overall)
    {
        var desiredByField = GetMultiSelectMap(overall);

        // Remove selections that are no longer present.
        var toRemove = account.MultiSelectValues
            .Where(existing => !desiredByField[existing.Field].Contains(existing.ReferenceItemId))
            .ToList();
        foreach (var value in toRemove)
        {
            account.MultiSelectValues.Remove(value);
        }

        // Add selections that are not yet stored.
        foreach (var (field, referenceItemIds) in desiredByField)
        {
            foreach (var referenceItemId in referenceItemIds.Distinct())
            {
                var alreadyPresent = account.MultiSelectValues
                    .Any(existing => existing.Field == field && existing.ReferenceItemId == referenceItemId);

                if (!alreadyPresent)
                {
                    account.MultiSelectValues.Add(new AccountMultiSelectValue
                    {
                        AccountId = account.Id,
                        Field = field,
                        ReferenceItemId = referenceItemId
                    });
                }
            }
        }
    }

    private AccountOverallDto MapToOverallDto(Account account)
    {
        var dto = new AccountOverallDto
        {
            Id = account.Id,
            Name = account.Name,
            RegionalOwnership = account.RegionalOwnership,
            GlobalSm = account.GlobalSm,
            GlobalSdm = account.GlobalSdm,
            EmeaLtsSdm = account.EmeaLtsSdm,
            NaLtsSdm = account.NaLtsSdm,
            EmeaSm = account.EmeaSm,
            EmeaSdm = account.EmeaSdm,
            NaSdm = account.NaSdm,
            ApacSdm = account.ApacSdm,
            Tsm = account.Tsm,
            BuLeader = account.BuLeader,
            TransformationManager = account.TransformationManager,
            AccountManagerCrd = account.AccountManagerCrd,
            LaunchDate = account.LaunchDate,
            ContractEndDate = account.ContractEndDate,
            AccountTypeRefId = account.AccountTypeRefId,
            FlagshipAccount = account.FlagshipAccount,
            IndustryRefId = account.IndustryRefId,
            HeadquarterCountryRefId = account.HeadquarterCountryRefId,
            ContractSupervisor = account.ContractSupervisor,
            NoOfUsersSupported = account.NoOfUsersSupported,
            SharingConstraints = account.SharingConstraints,
            ConnectivityRefId = account.ConnectivityRefId,
            TelecomCountry = account.TelecomCountry,
            TelecomRefId = account.TelecomRefId,
            ClientHardware = account.ClientHardware,
            ItsmToolRefId = account.ItsmToolRefId,
            ManagedByRefId = account.ManagedByRefId,
            ProjectTrainingDurationDays = account.ProjectTrainingDurationDays,
            SupportChannels = account.SupportChannels,
            WorkFromHomeApproved = account.WorkFromHomeApproved
        };

        // Populate the multi-select lists by field.
        foreach (var value in account.MultiSelectValues)
        {
            GetListForField(dto, value.Field).Add(value.ReferenceItemId);
        }

        return dto;
    }

    /// <summary>Maps each multi-select field to the list of selected ids carried by the DTO.</summary>
    private static IReadOnlyDictionary<MultiSelectField, List<int>> GetMultiSelectMap(AccountOverallDto dto) =>
        new Dictionary<MultiSelectField, List<int>>
        {
            [MultiSelectField.ContractType] = dto.ContractType,
            [MultiSelectField.BillingModel] = dto.BillingModel,
            [MultiSelectField.BillingModelLts] = dto.BillingModelLts,
            [MultiSelectField.BillingModelInfra] = dto.BillingModelInfra,
            [MultiSelectField.InScope] = dto.InScope,
            [MultiSelectField.TechnologySupported] = dto.TechnologySupported,
            [MultiSelectField.DevicesSupported] = dto.DevicesSupported
        };

    private static List<int> GetListForField(AccountOverallDto dto, MultiSelectField field) => field switch
    {
        MultiSelectField.ContractType => dto.ContractType,
        MultiSelectField.BillingModel => dto.BillingModel,
        MultiSelectField.BillingModelLts => dto.BillingModelLts,
        MultiSelectField.BillingModelInfra => dto.BillingModelInfra,
        MultiSelectField.InScope => dto.InScope,
        MultiSelectField.TechnologySupported => dto.TechnologySupported,
        MultiSelectField.DevicesSupported => dto.DevicesSupported,
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown multi-select field.")
    };
}
