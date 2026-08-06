using Battleship.Api.Services;
using Battleship.Api.Repositories;
using Moq;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Service_Tests;

public class SessionServiceTests
{
    private readonly Mock<ILobbyRepository> _mockLobbyRepository = new();
    private readonly Mock<IGameRepository> _mockGameRepository = new();
    private readonly SessionService _sessionService;

    public SessionServiceTests()
    {
        _sessionService = new SessionService(_mockLobbyRepository.Object, _mockGameRepository.Object);
    }
}

public class CollidingSessionService() : SessionService(
    new Mock<ILobbyRepository>().Object,
    new Mock<IGameRepository>().Object)
{
    private int _callCount = 0;

    protected override string GenerateCode()
    {
        _callCount++;
        if (_callCount == 1) return "DUPLIC";
        if (_callCount == 2) return "DUPLIC";
        return "UNIQUE";
    }
}