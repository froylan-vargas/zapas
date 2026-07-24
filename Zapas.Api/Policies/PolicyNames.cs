namespace Zapas.Api.Policies;

public static class PolicyNames
{
    public const string ZapasFrontend = "ZapasFrontend";
    public const string CanReadSessions = "CanReadSessions";
    public const string CanUploadSession = "CanUploadSession";
    public const string CanDeleteSession = "CanDeleteSession";
    public const string SessionUploadRateLimit = "session-upload";
}
