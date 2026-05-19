using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Zapas.Api.Repositories;
using Zapas.Api.Services;

namespace Zapas.Api.Tests.Services;

public sealed class SessionServiceValidationTests
{
    private readonly ISessionRepository _repository = Substitute.For<ISessionRepository>();
    private readonly IFitSessionParser _fitSessionParser = Substitute.For<IFitSessionParser>();
    private readonly SessionService _service;

    public SessionServiceValidationTests()
    {
        _service = new SessionService(
            _repository,
            _fitSessionParser,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<SessionService>.Instance);
    }

    [Fact]
    public async Task CreateSessionAsync_rejects_empty_file()
    {
        using var stream = Stream.Null;

        var result = await _service.CreateSessionAsync(
            stream,
            "activity.fit",
            fileLength: 0,
            CancellationToken.None);

        result.State.Should().Be(CreateSessionState.Rejected);
        result.Error.Should().Be("A non-empty .fit file is required.");
        _fitSessionParser.DidNotReceiveWithAnyArgs()
            .Parse(default!, default);
        await _repository.DidNotReceiveWithAnyArgs()
            .AddSessionAsync(default!, default);
    }

    [Fact]
    public async Task CreateSessionAsync_rejects_non_fit_extension()
    {
        using var stream = new MemoryStream([1, 2, 4]);
        var result = await _service.CreateSessionAsync(
            stream,
            "activity.txt",
            fileLength: stream.Length,
            CancellationToken.None
        );
        result.State.Should().Be(CreateSessionState.Rejected);
        result.Error.Should().Be("Only .fit files are supported.");
        _fitSessionParser.DidNotReceiveWithAnyArgs()
            .Parse(default!, default);
        await _repository.DidNotReceiveWithAnyArgs()
            .AddSessionAsync(default!, default);
    }

    [Fact]
    public async Task CreateSessionAsync_rejects_oversized_file()
    {
        using var stream = new MemoryStream([1]);
        const long maxFileSize = 3 * 1024 * 1024; // 3 MB

        var result = await _service.CreateSessionAsync(
            stream,
            "activity.fit",
            fileLength: maxFileSize + 1,
            CancellationToken.None
        );

        result.State.Should().Be(CreateSessionState.Rejected);
        result.Error.Should().Be("The uploaded file is too large.");
        _fitSessionParser.DidNotReceiveWithAnyArgs()
            .Parse(default!, default);
        await _repository.DidNotReceiveWithAnyArgs()
            .AddSessionAsync(default!, default);
    }
}
