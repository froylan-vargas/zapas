using Zapas.Api.Repositories;
using Zapas.Api.Services.CurrentUser;
using Zapas.Api.Services.FitParser;
using Zapas.Api.Services.Sessions;

namespace Zapas.Api.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddZapasApplicationServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IFitSessionParser, FitSessionParser>();

        return services;
    }
}
