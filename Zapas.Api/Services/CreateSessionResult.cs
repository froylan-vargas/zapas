using Zapas.Api.Models;

namespace Zapas.Api.Services;

public sealed record CreateSessionResult(
    CreateSessionState State,
    Session? Session,
    string? Error);
