using Zapas.Api.DTOs;
using Zapas.Api.Models;

namespace Zapas.Api.Services;

public interface ISessionService
{
    Task<IReadOnlyList<SessionSummary>> GetSessionsAsync(
        GetSessionsRequestDto request,
        CancellationToken cancellationToken);

    Task<Session?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CreateSessionResult> CreateSessionAsync(
        Stream fitStream,
        string? fileName,
        long fileLength,
        CancellationToken cancellationToken);

    Session ExtractSessionInfo(Stream fitStream, string? fallbackName = null);
}
