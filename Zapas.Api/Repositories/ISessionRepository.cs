using System;
using Zapas.Api.Models;

namespace Zapas.Api.Repositories;

public interface ISessionRepository
{
    IReadOnlyList<Session> GetAllSessions();
    Session? GetSessionById(Guid id);
    Session AddSession(Session session);
}
