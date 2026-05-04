using Zapas.Api.Models;

namespace Zapas.Api.Repositories;

public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly List<Session> _sessions = new();
    private readonly Lock _lock = new();
    
    public Session AddSession(Session session)
    {
        lock (_lock)
        {
            _sessions.Add(session);
        }

        return session;
    }

    public IReadOnlyList<Session> GetAllSessions()
    {
        lock (_lock)
        {
            return _sessions.ToList();
        }
    }

    public Session? GetSessionById(Guid id)
    {
        lock (_lock)
        {
            return _sessions.FirstOrDefault(session => session.Id == id);
        }
    }
}
