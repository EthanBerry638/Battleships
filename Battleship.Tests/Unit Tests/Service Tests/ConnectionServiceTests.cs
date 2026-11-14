using Battleship.Api.Services;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Repositories;
using Battleship.Api.Engine;
using Battleship.Api.Exceptions;
using Moq;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Service_Tests;

public class ConnectionServiceTests
{
    private readonly Mock<IConnectionRepository> _connectionRepositoryMock = new();
    private readonly Mock<IGameRepository> _gameRepositoryMock = new();
    private readonly Mock<ILobbyRepository> _lobbyRepositoryMock = new();
    private readonly ConnectionService _connectionService;
    private const string ConnectionId = "conn-123";

    public ConnectionServiceTests()
    {
        _connectionService = new ConnectionService(_connectionRepositoryMock.Object, _gameRepositoryMock.Object, _lobbyRepositoryMock.Object);
    }

    [Fact]
    public void RegisterConnection_ShouldMapConnectionToPlayer_WhenCalled()
    {
        Guid id = Guid.NewGuid();

        _connectionService.RegisterConnection(ConnectionId, id);

        _connectionRepositoryMock.Verify(r => r.SetConnection(ConnectionId, id), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryAddGame(It.IsAny<string>(), It.IsAny<GameSession>()), Times.Never);
        _lobbyRepositoryMock.Verify(r => r.TryAddLobby(It.IsAny<string>(), It.IsAny<Player>()), Times.Never);
    }

    [Fact]
    public void GetPlayerId_ShouldReturnPlayerId_WhenConnectionIsRegistered()
    {
        Guid id = Guid.NewGuid();
        _connectionRepositoryMock.Setup(r => r.TryGetPlayerId(ConnectionId, out id)).Returns(true);

        Guid result = _connectionService.GetPlayerId(ConnectionId);

        result.Should().Be(id);
    }

    [Fact]
    public void GetPlayerId_ShouldThrowPlayerNotFoundException_WhenConnectionIsNotRegistered()
    {
        Guid id = Guid.Empty;
        _connectionRepositoryMock.Setup(r => r.TryGetPlayerId(ConnectionId, out id)).Returns(false);

        Action act = () => _connectionService.GetPlayerId(ConnectionId);

        act.Should().Throw<PlayerNotFoundException>();
    }

    [Fact]
    public void GetConnectionIds_ShouldReturnEveryConnectionForEveryPlayer()
    {
        Guid player1 = Guid.NewGuid();
        Guid player2 = Guid.NewGuid();
        Guid player3 = Guid.NewGuid();
        _connectionRepositoryMock.Setup(r => r.GetConnectionIds(player1)).Returns(["conn-1a", "conn-1b"]);
        _connectionRepositoryMock.Setup(r => r.GetConnectionIds(player2)).Returns(["conn-2"]);
        _connectionRepositoryMock.Setup(r => r.GetConnectionIds(player3)).Returns([]);

        var result = _connectionService.GetConnectionIds([player1, player2, player3]);

        result.Should().BeEquivalentTo(["conn-1a", "conn-1b", "conn-2"]);
    }

    [Fact]
    public void EnsureConnectionIsFree_ShouldNotThrow_WhenConnectionIsNotRegistered()
    {
        Guid id = Guid.Empty;
        _connectionRepositoryMock.Setup(r => r.TryGetPlayerId(ConnectionId, out id)).Returns(false);

        Action act = () => _connectionService.EnsureConnectionIsFree(ConnectionId);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureConnectionIsFree_ShouldThrow_WhenConnectionsPlayerIsInLobby()
    {
        Guid id = Guid.NewGuid();
        _connectionRepositoryMock.Setup(r => r.TryGetPlayerId(ConnectionId, out id)).Returns(true);
        _lobbyRepositoryMock.Setup(r => r.IsPlayerInLobby(id)).Returns(true);

        Action act = () => _connectionService.EnsureConnectionIsFree(ConnectionId);

        act.Should().Throw<PlayerAlreadyInSessionException>();
    }

    [Fact]
    public void EnsureConnectionIsFree_ShouldThrow_WhenConnectionsPlayerIsInGame()
    {
        Guid id = Guid.NewGuid();
        _connectionRepositoryMock.Setup(r => r.TryGetPlayerId(ConnectionId, out id)).Returns(true);
        _gameRepositoryMock.Setup(r => r.IsPlayerInGame(id)).Returns(true);

        Action act = () => _connectionService.EnsureConnectionIsFree(ConnectionId);

        act.Should().Throw<PlayerAlreadyInSessionException>();
    }

    [Fact]
    public void EnsureConnectionIsFree_ShouldNotThrow_WhenConnectionsPlayerIsNotInLobbyOrGame()
    {
        Guid id = Guid.NewGuid();
        _connectionRepositoryMock.Setup(r => r.TryGetPlayerId(ConnectionId, out id)).Returns(true);
        _lobbyRepositoryMock.Setup(r => r.IsPlayerInLobby(id)).Returns(false);
        _gameRepositoryMock.Setup(r => r.IsPlayerInGame(id)).Returns(false);

        Action act = () => _connectionService.EnsureConnectionIsFree(ConnectionId);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task HandleDisconnectAsync_ShouldReturnNull_WhenConnectionIdIsNotFound()
    {
        var playerId = Guid.Empty;
        _connectionRepositoryMock
            .Setup(r => r.TryRemoveConnection(ConnectionId, out playerId))
            .Returns(false);

        string? result = await _connectionService.HandleDisconnectAsync(ConnectionId, TimeSpan.Zero);

        result.Should().BeNull();
        _connectionRepositoryMock.Verify(r => r.TryRemoveConnection(ConnectionId, out It.Ref<Guid>.IsAny), Times.Once);
        _connectionRepositoryMock.Verify(r => r.ContainsPlayer(It.IsAny<Guid>()), Times.Never);
        _lobbyRepositoryMock.Verify(r => r.TryFindCodeByPlayer(It.IsAny<Guid>(), out It.Ref<string?>.IsAny), Times.Never);
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(It.IsAny<Guid>(), out It.Ref<string?>.IsAny), Times.Never);
    }

    [Fact]
    public async Task HandleDisconnectAsync_ShouldReturnNull_WhenPlayerReconnectsWithinDelay()
    {
        var playerId = Guid.NewGuid();
        _connectionRepositoryMock
            .Setup(r => r.TryRemoveConnection(ConnectionId, out playerId))
            .Returns(true);
        _connectionRepositoryMock
            .Setup(r => r.ContainsPlayer(playerId))
            .Returns(true);

        string? result = await _connectionService.HandleDisconnectAsync(ConnectionId, TimeSpan.Zero);

        result.Should().BeNull();
        _lobbyRepositoryMock.Verify(r => r.TryFindCodeByPlayer(It.IsAny<Guid>(), out It.Ref<string?>.IsAny), Times.Never);
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(It.IsAny<Guid>(), out It.Ref<string?>.IsAny), Times.Never);
    }

    [Fact]
    public async Task HandleDisconnectAsync_ShouldReturnNull_AndRemoveLobby_WhenPlayerIsInLobby()
    {
        var playerId = Guid.NewGuid();
        string gameCode = "lobby-abc";
        _connectionRepositoryMock
            .Setup(r => r.TryRemoveConnection(ConnectionId, out playerId))
            .Returns(true);
        _connectionRepositoryMock
            .Setup(r => r.ContainsPlayer(playerId))
            .Returns(false);
        _lobbyRepositoryMock
            .Setup(r => r.TryFindCodeByPlayer(playerId, out gameCode!))
            .Returns(true);

        string? result = await _connectionService.HandleDisconnectAsync(ConnectionId, TimeSpan.Zero);

        result.Should().BeNull();
        _lobbyRepositoryMock.Verify(r => r.TryFindCodeByPlayer(playerId, out gameCode!), Times.Once);
        _lobbyRepositoryMock.Verify(r => r.TryRemoveLobby(gameCode, out It.Ref<Player?>.IsAny), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(It.IsAny<Guid>(), out It.Ref<string?>.IsAny), Times.Never);
    }

    [Fact]
    public async Task HandleDisconnectAsync_ShouldReturnNull_WhenPlayerIsNotInAnyGame()
    {
        var playerId = Guid.NewGuid();
        string? lobbyCode = null;
        string? gameCode = null;
        _connectionRepositoryMock
            .Setup(r => r.TryRemoveConnection(ConnectionId, out playerId))
            .Returns(true);
        _connectionRepositoryMock
            .Setup(r => r.ContainsPlayer(playerId))
            .Returns(false);
        _lobbyRepositoryMock
            .Setup(r => r.TryFindCodeByPlayer(playerId, out lobbyCode))
            .Returns(false);
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out gameCode))
            .Returns(false);

        string? result = await _connectionService.HandleDisconnectAsync(ConnectionId, TimeSpan.Zero);

        result.Should().BeNull();
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(playerId, out gameCode), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryRemoveGame(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleDisconnectAsync_ShouldReturnGameCode_AndRemoveGame_WhenPlayerIsInGame()
    {
        var playerId = Guid.NewGuid();
        string? lobbyCode = null;
        string gameCode = "game-xyz";
        _connectionRepositoryMock
            .Setup(r => r.TryRemoveConnection(ConnectionId, out playerId))
            .Returns(true);
        _connectionRepositoryMock
            .Setup(r => r.ContainsPlayer(playerId))
            .Returns(false);
        _lobbyRepositoryMock
            .Setup(r => r.TryFindCodeByPlayer(playerId, out lobbyCode!))
            .Returns(false);
        _gameRepositoryMock
            .Setup(r => r.TryFindKeyByPlayerId(playerId, out gameCode!))
            .Returns(true);

        string? result = await _connectionService.HandleDisconnectAsync(ConnectionId, TimeSpan.Zero);

        result.Should().Be("game-xyz");
        _gameRepositoryMock.Verify(r => r.TryFindKeyByPlayerId(playerId, out gameCode!), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryRemoveGame("game-xyz"), Times.Once);
    }
}