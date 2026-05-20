using Zapas.Api.Models;

namespace Zapas.Api.Services.Sessions;

public sealed record CreateSessionResult(
    CreateSessionState State,
    Session? Session,
    string? Error);
