using System.ComponentModel.DataAnnotations;

namespace Zapas.Api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;
}
