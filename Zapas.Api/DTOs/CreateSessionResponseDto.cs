namespace Zapas.Api.DTOs;

public sealed record CreateSessionResponseDto(
    string State,
    SessionDto? Session,
    string? Error);
