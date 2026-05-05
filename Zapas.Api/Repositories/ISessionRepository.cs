using Zapas.Api.DTOs;
using Zapas.Api.Models;

namespace Zapas.Api.Repositories;

public interface ISessionRepository
{
    Task<IReadOnlyList<SessionSummary>> GetSessionsAsync(
        GetSessionsRequestDto request,
        CancellationToken cancellationToken);

    Task<Session?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Session> AddSessionAsync(Session session, CancellationToken cancellationToken);
}
