using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zapas.Api.Data;

namespace Zapas.Api.Tests.Infrastructure;

public class UnavailableDatabaseZapasApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    internal string UnavailableDatabasePath { get; }
    public UnavailableDatabaseZapasApiFactory()
    {
        var missingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"zapas-health-{Guid.NewGuid():N}");

        UnavailableDatabasePath = Path.Combine(missingDirectory, "zapas.db");

        _connectionString =
            $"Data Source={UnavailableDatabasePath};Mode=ReadWrite;";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Keep the factory independent from local user-secrets and CI settings.
        builder.UseSetting(
            "Jwt:Authority",
            "https://identity.test.example");

        builder.UseSetting(
            "Jwt:Audience",
            "zapas-api-tests");

        builder.UseSetting(
            "Cors:AllowedOrigins:0",
            "https://frontend.test.example");


        builder.UseSetting(
            "ConnectionStrings:ZapasDb",
            "Data Source=:memory:");

        builder.ConfigureServices(services =>
          {
              services.RemoveAll<DbContextOptions<ZapasDbContext>>();
              services.RemoveAll<
                  IDbContextOptionsConfiguration<ZapasDbContext>>();

              services.AddDbContext<ZapasDbContext>(options =>
              {
                  options.UseSqlite(_connectionString);
              });
          });
    }
}
