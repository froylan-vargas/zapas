

namespace Zapas.Api.Options;

public sealed class UploadOptions
{
    public const string SectionName = "Uploads";

    public long MaxFitFileSizeBytes { get; init; } = 3 * 1024 * 1024;

    public string[] AllowedExtensions { get; init; } = [".fit"];
}
