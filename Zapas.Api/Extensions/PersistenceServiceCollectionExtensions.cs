using Microsoft.EntityFrameworkCore;
using Zapas.Api.Data;

namespace Zapas.Api.Extensions;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddZapasPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ZapasDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("ZapasDb"));
        });

        services
            .AddHealthChecks()
            .AddDbContextCheck<ZapasDbContext>(
                name: "zapas-database",
                tags: ["ready"]);

        return services;
    }
}
