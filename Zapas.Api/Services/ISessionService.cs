using Zapas.Api.Models;

namespace Zapas.Api.Services;

public interface ISessionService
{
    IReadOnlyList<Session> GetSessions();
    Session? GetSessionById(Guid id);
    CreateSessionResult CreateSession(Stream fitStream, string? fileName, long fileLength);
    Session ExtractSessionInfo(Stream fitStream, string? fallbackName = null);
}
