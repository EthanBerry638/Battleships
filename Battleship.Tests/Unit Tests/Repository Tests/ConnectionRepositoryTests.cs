using Battleship.Api.Repositories;
using Battleship.Api.DTOs;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Repository_Tests;

public class ConnectionRepositoryTests
{
    private readonly ConnectionRepository _connectionRepository = new();
    private const string Connection = "connection-1";
    
    [Fact]
    public void TryAddConnection_ShouldReturnTrue_WhenConnectionDoesNotExist()
    {
        bool result = _connectionRepository.TryAddConnection(Connection, Guid.NewGuid());

        result.Should().BeTrue();
    }

    [Fact]
    public void TryAddConnection_ShouldReturnFalse_WhenConnectionAlreadyExists()
    {
        Guid id = Guid.NewGuid();
        _connectionRepository.TryAddConnection(Connection, id);

        bool result = _connectionRepository.TryAddConnection(Connection, id);

        result.Should().BeFalse();
    }
    
    [Fact]
    public void TryRemoveConnection_ShouldReturnTrueAndOutputPlayerId_WhenConnectionExists()
    {
        Guid id = Guid.NewGuid();
        _connectionRepository.TryAddConnection(Connection, id);

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
    public void TryRemoveConnection_ShouldAllowReuse_WhenConnectionHasBeenRemoved()
    {
        Guid id = Guid.NewGuid();
        _connectionRepository.TryAddConnection(Connection, id);
        _connectionRepository.TryRemoveConnection(Connection, out _);

        bool result = _connectionRepository.TryAddConnection(Connection, id);

        result.Should().BeTrue();
    }
    
    [Fact]
    public void ContainsConnection_ShouldReturnTrue_WhenConnectionExists()
    {
        _connectionRepository.TryAddConnection(Connection, Guid.NewGuid());

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
        _connectionRepository.TryAddConnection(Connection, Guid.NewGuid());
        _connectionRepository.TryRemoveConnection(Connection, out _);

        bool result = _connectionRepository.ContainsConnection(Connection);

        result.Should().BeFalse();
    }
    
    [Fact]
    public void ContainsPlayer_ShouldReturnTrue_WhenPlayerExists()
    {
        var playerId = Guid.NewGuid();
        _connectionRepository.TryAddConnection(Connection, playerId);

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
        _connectionRepository.TryAddConnection(Connection, playerId);
        _connectionRepository.TryRemoveConnection(Connection, out _);

        bool result = _connectionRepository.ContainsPlayer(playerId);

        result.Should().BeFalse();
    }

    [Fact]
    public void ContainsPlayer_ShouldReturnTrue_WhenPlayerHasMultipleConnections()
    {
        var playerId = Guid.NewGuid();
        _connectionRepository.TryAddConnection(Connection, playerId);
        _connectionRepository.TryAddConnection("connection-2", playerId);
        _connectionRepository.TryRemoveConnection(Connection, out _);

        bool result = _connectionRepository.ContainsPlayer(playerId);

        result.Should().BeTrue();
    }
}