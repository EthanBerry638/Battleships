using Battleship.Api.Services;
using Battleship.Api.Repositories;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Board;
using Moq;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Service_Tests;

public class SessionServiceTests
{
    private readonly Mock<ILobbyRepository> _mockLobbyRepository = new();
    private readonly Mock<IGameRepository> _mockGameRepository = new();
    private readonly SessionService _sessionService;
    private readonly Player _dummyPlayer1 = new(Guid.NewGuid(), "Player 1");
    private readonly Player _dummyPlayer2 = new(Guid.NewGuid(), "Player 2");
    
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
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public void GetGame_ShouldReturnNull_WhenGameCodeIsNullOrWhitespace(string? gameCode)
    {
        var result = _sessionService.GetGame(gameCode!);

        result.Should().BeNull();
        _mockGameRepository.Verify(r => r.TryGetGameByCode(It.IsAny<string>(), out It.Ref<GameSession?>.IsAny), Times.Never);
    }

    [Fact]
    public void GetGame_ShouldReturnNull_WhenGameSessionNotFound()
    {
        GameSession? nullSession = null;
        _mockGameRepository
            .Setup(r => r.TryGetGameByCode(It.IsAny<string>(), out nullSession))
            .Returns(false);

        var result = _sessionService.GetGame("ABC123");

        result.Should().BeNull();
        _mockGameRepository.Verify(r => r.TryGetGameByCode(It.IsAny<string>(), out It.Ref<GameSession?>.IsAny), Times.Once);
    }

    [Fact]
    public void GetGame_ShouldReturnEngine_WhenGameSessionFound()
    {
        var dummyEngine = new BattleshipEngine(new GameBoard(), new GameBoard(), _dummyPlayer1, _dummyPlayer2);
        var dummySession = new GameSession (dummyEngine);
        _mockGameRepository
            .Setup(r => r.TryGetGameByCode("ABC123", out dummySession))
            .Returns(true);

        var result = _sessionService.GetGame("ABC123");

        result.Should().Be(dummyEngine);
        _mockGameRepository.Verify(r => r.TryGetGameByCode("ABC123", out It.Ref<GameSession?>.IsAny), Times.Once);
    }
    
    [Fact]
    public void JoinLobby_ShouldThrowArgumentNullException_WhenPlayerIsNull()
    {
        var act = () => _sessionService.JoinLobby("ABC123", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void JoinLobby_ShouldThrowPlayerAlreadyInSessionException_WhenPlayerAlreadyInLobby()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer2.Id)).Returns(true);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer2.Id)).Returns(false);

        var act = () => _sessionService.JoinLobby("ABC123", _dummyPlayer2);

        act.Should().Throw<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
        _mockLobbyRepository.Verify(r => r.IsPlayerInLobby(_dummyPlayer2.Id), Times.Once);
        _mockGameRepository.Verify(r => r.IsPlayerInGame(_dummyPlayer2.Id), Times.Once);
    }

    [Fact]
    public void JoinLobby_ShouldThrowPlayerAlreadyInSessionException_WhenPlayerAlreadyInGame()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer2.Id)).Returns(false);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer2.Id)).Returns(true);

        var act = () => _sessionService.JoinLobby("ABC123", _dummyPlayer2);

        act.Should().Throw<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
        _mockLobbyRepository.Verify(r => r.IsPlayerInLobby(_dummyPlayer2.Id), Times.Once);
        _mockGameRepository.Verify(r => r.IsPlayerInGame(_dummyPlayer2.Id), Times.Once);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    public void JoinLobby_ShouldReturnNull_WhenGameCodeIsNullOrWhitespace(string? gameCode)
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer2.Id)).Returns(false);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer2.Id)).Returns(false);

        var result = _sessionService.JoinLobby(gameCode!, _dummyPlayer2);

        result.Should().BeNull();
        _mockLobbyRepository.Verify(r => r.IsPlayerInLobby(It.IsAny<Guid>()), Times.Once);
        _mockGameRepository.Verify(r => r.IsPlayerInGame(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public void JoinLobby_ShouldReturnNull_WhenLobbyNotFound()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer2.Id)).Returns(false);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer2.Id)).Returns(false);
        Player? nullPlayer = null;
        _mockLobbyRepository.Setup(r => r.TryRemoveLobby("ABC123", out nullPlayer)).Returns(false);

        var result = _sessionService.JoinLobby("ABC123", _dummyPlayer2);

        result.Should().BeNull();
        _mockLobbyRepository.Verify(r => r.IsPlayerInLobby(It.IsAny<Guid>()), Times.Once);
        _mockGameRepository.Verify(r => r.IsPlayerInGame(It.IsAny<Guid>()), Times.Once);
        _mockLobbyRepository.Verify(r => r.TryRemoveLobby(It.IsAny<string>(), out It.Ref<Player?>.IsAny), Times.Once);
        _mockGameRepository.Verify(r => r.IsPlayerInGame(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public void JoinLobby_ShouldReturnEngine_WhenLobbyFound()
    {
        _mockLobbyRepository.Setup(r => r.IsPlayerInLobby(_dummyPlayer2.Id)).Returns(false);
        _mockGameRepository.Setup(r => r.IsPlayerInGame(_dummyPlayer2.Id)).Returns(false);
        Player? player1 = _dummyPlayer1;
        _mockLobbyRepository.Setup(r => r.TryRemoveLobby("ABC123", out player1)).Returns(true);
        _mockGameRepository.Setup(r => r.TryAddGame(It.IsAny<string>(), It.IsAny<GameSession>())).Returns(true);

        var result = _sessionService.JoinLobby("ABC123", _dummyPlayer2);

        result.Should().NotBeNull();
        result.Should().BeOfType<BattleshipEngine>();
        _mockLobbyRepository.Verify(r => r.IsPlayerInLobby(It.IsAny<Guid>()), Times.Once);
        _mockGameRepository.Verify(r => r.IsPlayerInGame(It.IsAny<Guid>()), Times.Once);
        _mockLobbyRepository.Verify(r => r.TryRemoveLobby(It.IsAny<string>(), out It.Ref<Player?>.IsAny), Times.Once);
        _mockGameRepository.Verify(r => r.TryAddGame(It.IsAny<string>(), It.IsAny<GameSession>()), Times.Once);
    }
}