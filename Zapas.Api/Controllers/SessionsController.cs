using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zapas.Api.DTOs;
using Zapas.Api.Services;

namespace Zapas.Api.Controllers;

[ApiController]
[Route("sessions")]
public sealed class SessionsController : ControllerBase
{
    private readonly ILogger<SessionsController> _logger;
    private readonly ISessionService _sessionService;

    public SessionsController(
        ILogger<SessionsController> logger,
        ISessionService sessionService)
    {
        _logger = logger;
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions(
        [FromQuery] GetSessionsRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching sessions");
        var sessions = await _sessionService.GetSessionsAsync(request, cancellationToken);
        return Ok(sessions
            .Select(SessionSummaryDto.FromModel)
            .ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSession(Guid id, CancellationToken cancellationToken)
    {
        var session = await _sessionService.GetSessionByIdAsync(id, cancellationToken);

        if (session is null)
        {
            return NotFound();
        }

        return Ok(SessionDto.FromModel(session));
    }

    [RequestSizeLimit(3 * 1024 * 1024)]
    [EnableRateLimiting("session-upload")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateSession(IFormFile? file, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating a new session from uploaded FIT file");

        using var fitStream = file?.OpenReadStream() ?? Stream.Null;
        var result = await _sessionService.CreateSessionAsync(
            fitStream,
            file?.FileName,
            file?.Length ?? 0,
            cancellationToken);
        var response = ToCreateSessionResponse(result);

        return result.State switch
        {
            CreateSessionState.Stored => CreatedAtAction(
                nameof(GetSession),
                new { id = response.Session?.Id },
                response),
            CreateSessionState.Rejected => BadRequest(response),
            CreateSessionState.Failed => BadRequest(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response)
        };
    }

    private static CreateSessionResponseDto ToCreateSessionResponse(CreateSessionResult result)
    {
        return new CreateSessionResponseDto(
            State: result.State.ToString(),
            Session: result.Session is null ? null : SessionDto.FromModel(result.Session),
            Error: result.Error);
    }
}
