using AidDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AidDashboard.Api;

/// <summary>
/// Lets the EF Core CLI create the DbContext at design time (for migrations) without
/// booting the full web host. Uses a local SQLite file; the real connection string is
/// supplied at runtime via configuration.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=aid-dashboard.db")
            .Options;
        return new AppDbContext(options);
    }
}
