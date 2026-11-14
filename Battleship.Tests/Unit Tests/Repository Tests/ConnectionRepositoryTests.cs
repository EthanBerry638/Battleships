using Battleship.Api.Repositories;
using Battleship.Api.DTOs;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Repository_Tests;

public class ConnectionRepositoryTests
{
    private readonly ConnectionRepository _connectionRepository = new();
    private const string Connection = "connection-1";
    
    [Fact]
    public void SetConnection_ShouldMapConnectionToPlayer_WhenConnectionDoesNotExist()
    {
        Guid id = Guid.NewGuid();

        _connectionRepository.SetConnection(Connection, id);

        _connectionRepository.TryGetPlayerId(Connection, out Guid playerId).Should().BeTrue();
        playerId.Should().Be(id);
    }

    [Fact]
    public void SetConnection_ShouldReplacePlayer_WhenConnectionAlreadyExists()
    {
        Guid oldId = Guid.NewGuid();
        Guid newId = Guid.NewGuid();
        _connectionRepository.SetConnection(Connection, oldId);

        _connectionRepository.SetConnection(Connection, newId);

        _connectionRepository.TryGetPlayerId(Connection, out Guid playerId).Should().BeTrue();
        playerId.Should().Be(newId);
        _connectionRepository.ContainsPlayer(oldId).Should().BeFalse();
    }

    [Fact]
    public void TryGetPlayerId_ShouldReturnFalse_WhenConnectionDoesNotExist()
    {
        bool result = _connectionRepository.TryGetPlayerId(Connection, out Guid playerId);

        result.Should().BeFalse();
        playerId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TryRemoveConnection_ShouldReturnTrueAndOutputPlayerId_WhenConnectionExists()
    {
        Guid id = Guid.NewGuid();
        _connectionRepository.SetConnection(Connection, id);

        bool result = _connectionRepository.TryRemoveConnection(Connection, out Guid removedPlayerId);

        result.Should().BeTrue();
        removedPlayerId.Should().Be(id);
    }

    [Fact]
    public void TryRemoveConnection_ShouldReturnFalse_WhenConnectionDoesNotExist()
    {
        bool result = _connectionRepository.TryRemoveConnection(Connection, out Guid removedPlayerId);

        result.Should().BeFalse();
        removedPlayerId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TryRemoveConnection_ShouldRemovePlayerLookup_WhenConnectionHasBeenRemoved()
    {
        Guid id = Guid.NewGuid();
        _connectionRepository.SetConnection(Connection, id);
        _connectionRepository.TryRemoveConnection(Connection, out _);

        bool result = _connectionRepository.TryGetPlayerId(Connection, out _);

        result.Should().BeFalse();
    }
    
    [Fact]
    public void ContainsConnection_ShouldReturnTrue_WhenConnectionExists()
    {
        _connectionRepository.SetConnection(Connection, Guid.NewGuid());

        bool result = _connectionRepository.ContainsConnection(Connection);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsConnection_ShouldReturnFalse_WhenConnectionDoesNotExist()
    {
        bool result = _connectionRepository.ContainsConnection("connection-1");

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsConnection_ShouldReturnFalse_WhenConnectionHasBeenRemoved()
    {
        _connectionRepository.SetConnection(Connection, Guid.NewGuid());
        _connectionRepository.TryRemoveConnection(Connection, out _);

        bool result = _connectionRepository.ContainsConnection(Connection);

        result.Should().BeFalse();
    }
    
    [Fact]
    public void ContainsPlayer_ShouldReturnTrue_WhenPlayerExists()
    {
        var playerId = Guid.NewGuid();
        _connectionRepository.SetConnection(Connection, playerId);

        bool result = _connectionRepository.ContainsPlayer(playerId);

        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsPlayer_ShouldReturnFalse_WhenPlayerDoesNotExist()
    {
        bool result = _connectionRepository.ContainsPlayer(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPlayer_ShouldReturnFalse_WhenPlayerHasBeenRemoved()
    {
        var playerId = Guid.NewGuid();
        _connectionRepository.SetConnection(Connection, playerId);
        _connectionRepository.TryRemoveConnection(Connection, out _);

        bool result = _connectionRepository.ContainsPlayer(playerId);

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPlayer_ShouldReturnTrue_WhenPlayerHasMultipleConnections()
    {
        var playerId = Guid.NewGuid();
        _connectionRepository.SetConnection(Connection, playerId);
        _connectionRepository.SetConnection("connection-2", playerId);
        _connectionRepository.TryRemoveConnection(Connection, out _);

        bool result = _connectionRepository.ContainsPlayer(playerId);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetConnectionIds_ShouldReturnEmpty_WhenPlayerHasNoConnections()
    {
        _connectionRepository.SetConnection(Connection, Guid.NewGuid());

        var result = _connectionRepository.GetConnectionIds(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetConnectionIds_ShouldReturnOnlyThatPlayersConnection_WhenOtherPlayersAreConnected()
    {
        var playerId = Guid.NewGuid();
        _connectionRepository.SetConnection(Connection, playerId);
        _connectionRepository.SetConnection("connection-2", Guid.NewGuid());

        var result = _connectionRepository.GetConnectionIds(playerId);

        result.Should().Equal(Connection);
    }

    [Fact]
    public void GetConnectionIds_ShouldReturnAllConnections_WhenPlayerHasMultipleConnections()
    {
        var playerId = Guid.NewGuid();
        _connectionRepository.SetConnection(Connection, playerId);
        _connectionRepository.SetConnection("connection-2", playerId);

        var result = _connectionRepository.GetConnectionIds(playerId);

        result.Should().BeEquivalentTo([Connection, "connection-2"]);
    }
}