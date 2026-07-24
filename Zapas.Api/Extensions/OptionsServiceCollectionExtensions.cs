using Zapas.Api.Options;

namespace Zapas.Api.Extensions;

public static class OptionsServiceCollectionExtensions
{
    public static IServiceCollection AddZapasOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<UploadOptions>()
            .Bind(configuration.GetSection(UploadOptions.SectionName))
            .Validate(
                options => options.MaxFitFileSizeBytes > 0,
                "Uploads:MaxFitFileSizeBytes must be greater than zero.")
            .Validate(
                options => options.AllowedExtensions.Length > 0 &&
                           options.AllowedExtensions.All(extension =>
                               extension.StartsWith('.') &&
                               !string.IsNullOrWhiteSpace(extension)),
                "Uploads:AllowedExtensions must contain at least one dot-prefixed extension.")
            .ValidateOnStart();

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => Uri.TryCreate(options.Authority, UriKind.Absolute, out var uri) &&
                           uri.Scheme == Uri.UriSchemeHttps,
                "Jwt:Authority must be an absolute HTTPS URI.")
            .ValidateOnStart();

        services
            .AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .Validate(
                options => options.AllowedOrigins.All(origin =>
                    Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttps ||
                     uri.Host is "localhost" or "127.0.0.1")),
                "CORS origins must be absolute HTTPS URIs; HTTP is allowed only for local development.")
            .ValidateOnStart();

        return services;
    }
}
