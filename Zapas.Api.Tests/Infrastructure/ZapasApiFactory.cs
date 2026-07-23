using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zapas.Api.Data;

namespace Zapas.Api.Tests.Infrastructure;

public sealed class ZapasApiFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ZapasDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ZapasDbContext>>();

            services.AddDbContext<ZapasDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ZapasDbContext>();
            dbContext.Database.EnsureCreated();
        });

        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<TestAuthOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme,
                    _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
