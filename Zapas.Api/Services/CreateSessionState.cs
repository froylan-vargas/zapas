namespace Zapas.Api.Services;

public enum CreateSessionState
{
    Received,
    Rejected,
    Extracted,
    Stored,
    Failed
}
