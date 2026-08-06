using Battleship.Api.Services;
using Battleship.Api.Repositories;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Entities;
using Moq;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Service_Tests;

public class SessionServiceTests
{
    private readonly Mock<ILobbyRepository> _mockLobbyRepository = new();
    private readonly Mock<IGameRepository> _mockGameRepository = new();
    private readonly SessionService _sessionService;
    private readonly Player _dummyPlayer1 = new(Guid.NewGuid(), "Player 1");

    public SessionServiceTests()
    {
        _sessionService = new SessionService(_mockLobbyRepository.Object, _mockGameRepository.Object);
    }

    [Fact]
    public void CreateLobby_ShouldReturnSixCharacterCode_WhenCalled()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer1.Id)).Returns(false);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer1.Id)).Returns(false);
        _mockLobbyRepository.Setup(r => r.TryAddLobby(It.IsAny<string>(), _dummyPlayer1)).Returns(true);

        string result = _sessionService.CreateLobby(_dummyPlayer1);

        result.Length.Should().Be(6);
        result.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CreateLobby_ShouldReturnUppercaseCode_WhenCalled()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer1.Id)).Returns(false);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer1.Id)).Returns(false);
        _mockLobbyRepository.Setup(r => r.TryAddLobby(It.IsAny<string>(), _dummyPlayer1)).Returns(true);

        string result = _sessionService.CreateLobby(_dummyPlayer1);

        result.Should().BeUpperCased();
    }

    [Fact]
    public void CreateLobby_ShouldThrowArgumentNullException_WhenPlayerIsNull()
    {
        var act = () => _sessionService.CreateLobby(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateLobby_ShouldRetryGeneration_WhenCodeCollisionOccurs()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(It.IsAny<Guid>())).Returns(false); 
        _mockGameRepository.Setup(r => r.IsPlayerInGame(It.IsAny<Guid>())).Returns(false);
        _mockLobbyRepository
            .SetupSequence(r => r.TryAddLobby(It.IsAny<string>(), It.IsAny<Player>()))
            .Returns(false)
            .Returns(false)
            .Returns(true);

        string result = _sessionService.CreateLobby(_dummyPlayer1);

        result.Should().NotBeNullOrEmpty();
        result.Should().HaveLength(6);
        _mockLobbyRepository.Verify(r => r.TryAddLobby(It.IsAny<string>(), It.IsAny<Player>()), Times.Exactly(3));
    }

    [Fact]
    public void CreateLobby_ShouldThrowPlayerAlreadyInSessionException_WhenPlayerAlreadyHasOpenLobby()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer1.Id)).Returns(true);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer1.Id)).Returns(false);

        var act = () => _sessionService.CreateLobby(_dummyPlayer1);

        act.Should().Throw<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
        _mockLobbyRepository.Verify(r => r.IsPlayerInLobby(_dummyPlayer1.Id), Times.Once);
    }

    [Fact]
    public void CreateLobby_ShouldThrowPlayerAlreadyInSessionException_WhenPlayerAlreadyInActiveGame()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer1.Id)).Returns(false);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer1.Id)).Returns(true);

        var act = () => _sessionService.CreateLobby(_dummyPlayer1);

        act.Should().Throw<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
        _mockGameRepository.Verify(r => r.IsPlayerInGame(_dummyPlayer1.Id), Times.Once);
    }
}