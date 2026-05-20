namespace Zapas.Api.Services.Sessions;

public enum CreateSessionState
{
    Received,
    Rejected,
    Extracted,
    Stored,
    Failed
}
