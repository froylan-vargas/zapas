using Zapas.Api.DTOs;
using Zapas.Api.Models;

namespace Zapas.Api.Repositories;

public sealed class InMemorySessionRepository : ISessionRepository
{
    private readonly List<Session> _sessions = new();
    private readonly Lock _lock = new();

    public Task<Session> AddSessionAsync(Session session, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            _sessions.Add(session);
        }

        return Task.FromResult(session);
    }

    public Task<IReadOnlyList<SessionSummary>> GetSessionsAsync(
        GetSessionsRequestDto request,
        CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            var page = Math.Max(request.Page, 1);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            IEnumerable<Session> query = _sessions;

            if (request.From is not null)
            {
                query = query.Where(session => session.StartTime >= request.From);
            }

            if (request.To is not null)
            {
                query = query.Where(session => session.StartTime <= request.To);
            }

            if (request.HasIntervals is not null)
            {
                query = request.HasIntervals.Value
                    ? query.Where(session => session.RunIntervals.Count > 0)
                    : query.Where(session => session.RunIntervals.Count == 0);
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(session => session.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase));
            }

            query = request.Sort?.ToLowerInvariant() switch
            {
                "starttime" => query.OrderByDescending(session => session.StartTime),
                "distance" => query.OrderByDescending(session => session.TotalDistance),
                _ => query.OrderByDescending(session => session.StartTime)
            };

            var sessions = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(session => new SessionSummary(
                    session.Id,
                    session.Name,
                    session.TotalDistance,
                    session.TotalDuration,
                    session.AveragePace,
                    session.AverageHeartRate,
                    session.MaxHeartRate,
                    session.StartTime,
                    session.CreatedAt))
                .ToList();

            return Task.FromResult<IReadOnlyList<SessionSummary>>(sessions);
        }
    }

    public Task<Session?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            return Task.FromResult(_sessions.FirstOrDefault(session => session.Id == id));
        }
    }
}
