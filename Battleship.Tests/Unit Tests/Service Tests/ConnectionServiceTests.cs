using Battleship.Api.Services;
using Battleship.Api.DTOs;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Repositories;
using Battleship.Api.Engine;
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
    public void AddConnection_ShouldReturnTrue_WhenConnectionIsValid()
    {
        var request = new AddConnectionRequest(ConnectionId, Guid.NewGuid());
        _connectionRepositoryMock.Setup(r => r.TryAddConnection(request)).Returns(true);

        bool result = _connectionService.AddConnection(request);
        
        result.Should().BeTrue();
        _connectionRepositoryMock.Verify(r => r.TryAddConnection(request), Times.Once);
        _gameRepositoryMock.Verify(r => r.TryAddGame(It.IsAny<string>(), It.IsAny<GameSession>()), Times.Never);
        _lobbyRepositoryMock.Verify(r => r.TryAddLobby(It.IsAny<string>(), It.IsAny<Player>()), Times.Never);
    }

    [Fact]
    public void AddConnection_ShouldReturnFalse_WhenConnectionAlreadyExists()
    {
        var request = new AddConnectionRequest(ConnectionId, Guid.NewGuid());
        _connectionRepositoryMock.Setup(r => r.TryAddConnection(request)).Returns(false);

        bool result = _connectionService.AddConnection(request);

        result.Should().BeFalse();
        _connectionRepositoryMock.Verify(r => r.TryAddConnection(request), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddConnection_ShouldThrowArgumentException_WhenConnectionIdIsNullOrWhiteSpace(string? connectionId)
    {
        var request = new AddConnectionRequest(connectionId!, Guid.NewGuid());

        var act = () => _connectionService.AddConnection(request);

        act.Should().Throw<ArgumentException>()
            .WithMessage("ConnectionId and/or Guid cannot be null or empty.");
        _connectionRepositoryMock.Verify(r => r.TryAddConnection(It.IsAny<AddConnectionRequest>()), Times.Never);
    }

    [Fact]
    public void AddConnection_ShouldThrowArgumentException_WhenPlayerIdIsEmpty()
    {
        var request = new AddConnectionRequest(ConnectionId, Guid.Empty);

        var act = () => _connectionService.AddConnection(request);

        act.Should().Throw<ArgumentException>()
            .WithMessage("ConnectionId and/or Guid cannot be null or empty.");
        _connectionRepositoryMock.Verify(r => r.TryAddConnection(It.IsAny<AddConnectionRequest>()), Times.Never);
    }
    
    [Fact]
    public void AddConnection_ShouldThrowArgumentException_WhenBothConnectionIdAndPlayerIdAreInvalid()
    {
        var request = new AddConnectionRequest(null!, Guid.Empty);

        var act = () => _connectionService.AddConnection(request);

        act.Should().Throw<ArgumentException>()
            .WithMessage("ConnectionId and/or Guid cannot be null or empty.");
        _connectionRepositoryMock.Verify(r => r.TryAddConnection(It.IsAny<AddConnectionRequest>()), Times.Never);
    }

    [Fact]
    public void AddConnection_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        var act = () => _connectionService.AddConnection(null!);

        act.Should().Throw<ArgumentNullException>();
        _connectionRepositoryMock.Verify(r => r.TryAddConnection(It.IsAny<AddConnectionRequest>()), Times.Never);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleDisconnectAsync_ShouldThrowArgumentException_WhenConnectionIdIsNullOrWhiteSpace(string? connectionId)
    {
        var act = async () => await _connectionService.HandleDisconnectAsync(connectionId!);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Connection ID cannot be null or whitespace");
        _connectionRepositoryMock.Verify(r => r.TryRemoveConnection(It.IsAny<string>(), out It.Ref<Guid>.IsAny), Times.Never);
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