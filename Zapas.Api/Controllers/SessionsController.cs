using Microsoft.AspNetCore.Mvc;
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
    public IActionResult GetSessions()
    {
        _logger.LogInformation("Fetching sessions");
        var sessions = _sessionService.GetSessions()
            .Select(SessionDto.FromModel)
            .ToList();

        return Ok(sessions);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetSession(Guid id)
    {
        var session = _sessionService.GetSessionById(id);

        if (session is null)
        {
            return NotFound();
        }

        return Ok(SessionDto.FromModel(session));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public IActionResult CreateSession(IFormFile? file)
    {
        _logger.LogInformation("Creating a new session from uploaded FIT file");

        using var fitStream = file?.OpenReadStream() ?? Stream.Null;
        var result = _sessionService.CreateSession(fitStream, file?.FileName, file?.Length ?? 0);
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
