using AidDashboard.Application.Abstractions;
using AidDashboard.Application.Accounts;
using AidDashboard.Application.Admin;
using AidDashboard.Application.Chat;
using AidDashboard.Application.Dashboard;
using AidDashboard.Application.ImportExport;
using AidDashboard.Application.Reference;
using AidDashboard.Application.Services;
using AidDashboard.Application.Tabs;
using AidDashboard.Domain.Accounts;
using AidDashboard.Infrastructure.Auth;
using AidDashboard.Infrastructure.ImportExport;
using AidDashboard.Infrastructure.ImportExport.Providers;
using AidDashboard.Infrastructure.Persistence;
using AidDashboard.Infrastructure.Security;
using AidDashboard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AidDashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("Default") ?? "Data Source=aid-dashboard.db";
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(conn));

        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // Active authentication provider. Swap/extend here to add Entra ID later.
        services.AddScoped<IAuthenticationProvider, LocalAuthenticationProvider>();
        services.AddScoped<IAuthService, AuthService>();

        // Reference data, accounts and admin user management.
        services.AddScoped<IReferenceService, ReferenceService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUserAdminService, UserAdminService>();

        // Per-tab data services.
        services.AddScoped<IStaffingService, StaffingService>();
        services.AddScoped<IContractualLanguageService, ContractualLanguageService>();
        services.AddScoped<ICountryDeviceService, CountryDeviceService>();
        services.AddScoped<ISupportHourService, SupportHourService>();
        services.AddScoped<IAutomationService, AutomationService>();
        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<ISlaKpiService, SlaKpiService>();

        // Services hierarchy + per-account services data.
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<IAccountServicesService, AccountServicesService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // AI assistant (Azure OpenAI chat). Needs an HttpClient; settings are read at call time.
        services.AddHttpClient();
        services.AddScoped<IChatService, ChatService>();

        // Import/export: the serializer renders/parses files; each provider maps one tab.
        services.AddSingleton<ITableSerializer, TableSerializer>();
        services.AddScoped<ITabTableProvider, AutomationsTableProvider>();
        services.AddScoped<ITabTableProvider, OpportunitiesTableProvider>();
        services.AddScoped<ITabTableProvider, SlaKpisTableProvider>();
        services.AddScoped<ITabTableProvider, SupportHoursTableProvider>();
        services.AddScoped<ITabTableProvider, ContractualLanguagesTableProvider>();
        services.AddScoped<ITabTableProvider, CountriesDevicesTableProvider>();
        services.AddScoped<ITabTableProvider>(serviceProvider => new StaffingTableProvider(
            serviceProvider.GetRequiredService<IStaffingService>(),
            serviceProvider.GetRequiredService<IReferenceService>(),
            StaffingModelType.Initial));
        services.AddScoped<ITabTableProvider>(serviceProvider => new StaffingTableProvider(
            serviceProvider.GetRequiredService<IStaffingService>(),
            serviceProvider.GetRequiredService<IReferenceService>(),
            StaffingModelType.LatestApproved));
        services.AddScoped<ITabTableProvider, ServicesTableProvider>();

        return services;
    }
}
