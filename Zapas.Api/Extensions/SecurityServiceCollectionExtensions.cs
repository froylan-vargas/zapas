using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Zapas.Api.Options;
using Zapas.Api.Policies;

namespace Zapas.Api.Extensions;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddZapasSecurity(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var jwtOptions = configuration
            .GetRequiredSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException("The Jwt section is required.");

        var corsOptions = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyNames.ZapasFrontend, policy =>
            {
                policy.WithOrigins(corsOptions.AllowedOrigins)
                    .WithMethods("GET", "POST", "DELETE")
                    .WithHeaders("Content-Type", "Authorization");
            });
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = jwtOptions.Authority;
                options.Audience = jwtOptions.Audience;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
            });

        AddAuthorization(services);
        AddRateLimiting(services);

        return services;
    }

    private static void AddAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyNames.CanReadSessions, policy =>
            {
                policy.RequireAuthenticatedUser();
            });

            options.AddPolicy(PolicyNames.CanUploadSession, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Athlete");
            });

            options.AddPolicy(PolicyNames.CanDeleteSession, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole("Admin");
            });
        });
    }

    private static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(PolicyNames.SessionUploadRateLimit, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });
    }
}
