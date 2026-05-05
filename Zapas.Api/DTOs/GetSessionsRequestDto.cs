namespace Zapas.Api.DTOs;

public sealed record GetSessionsRequestDto(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    bool? HasIntervals = null,
    string? Name = null);
