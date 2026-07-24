using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Zapas.Api.Middleware;
using Zapas.Api.Policies;

namespace Zapas.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseZapasPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseHttpsRedirection();
        app.UseCors(PolicyNames.ZapasFrontend);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapControllers();
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        });

        return app;
    }
}
