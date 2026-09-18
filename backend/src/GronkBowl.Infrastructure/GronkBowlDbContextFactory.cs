using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GronkBowl.Infrastructure;

/// <summary>Lets `dotnet ef migrations` run without needing the Api project's full DI setup.
/// The connection string here is dev-only, matching the local Docker Postgres container.</summary>
public class GronkBowlDbContextFactory : IDesignTimeDbContextFactory<GronkBowlDbContext>
{
    public const string DevConnectionString =
        "Host=localhost;Port=5433;Database=gronkbowl;Username=gronkbowl;Password=gronkbowl_dev_local";

    public GronkBowlDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GronkBowlDbContext>()
            .UseNpgsql(DevConnectionString)
            .Options;

        return new GronkBowlDbContext(options);
    }
}
