using Battleship.Api.Repositories;
using Battleship.Api.DTOs;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Repository_Tests;

public class ConnectionRepositoryTests
{
    private readonly ConnectionRepository _connectionRepository = new();
    
    [Fact]
    public void TryAddConnection_ShouldReturnTrue_WhenConnectionDoesNotExist()
    {
        var request = new AddConnectionRequest("connection-1", Guid.NewGuid());

        bool result = _connectionRepository.TryAddConnection(request);

        result.Should().BeTrue();
    }

    [Fact]
    public void TryAddConnection_ShouldReturnFalse_WhenConnectionAlreadyExists()
    {
        var request = new AddConnectionRequest("connection-1", Guid.NewGuid());
        _connectionRepository.TryAddConnection(request);

        bool result = _connectionRepository.TryAddConnection(request);

        result.Should().BeFalse();
    }
    
    [Fact]
    public void TryRemoveConnection_ShouldReturnTrueAndOutputPlayerId_WhenConnectionExists()
    {
        var playerId = Guid.NewGuid();
        _connectionRepository.TryAddConnection(new AddConnectionRequest("connection-1", playerId));

        bool result = _connectionRepository.TryRemoveConnection("connection-1", out Guid removedPlayerId);

        result.Should().BeTrue();
        removedPlayerId.Should().Be(playerId);
    }

    [Fact]
    public void TryRemoveConnection_ShouldReturnFalse_WhenConnectionDoesNotExist()
    {
        bool result = _connectionRepository.TryRemoveConnection("connection-1", out Guid removedPlayerId);

        result.Should().BeFalse();
        removedPlayerId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TryRemoveConnection_ShouldAllowReuse_WhenConnectionHasBeenRemoved()
    {
        var request = new AddConnectionRequest("connection-1", Guid.NewGuid());
        _connectionRepository.TryAddConnection(request);
        _connectionRepository.TryRemoveConnection("connection-1", out _);

        bool result = _connectionRepository.TryAddConnection(request);

        result.Should().BeTrue();
    }
    
    [Fact]
    public void ContainsConnection_ShouldReturnTrue_WhenConnectionExists()
    {
        _connectionRepository.TryAddConnection(new AddConnectionRequest("connection-1", Guid.NewGuid()));

        bool result = _connectionRepository.ContainsConnection("connection-1");

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
        _connectionRepository.TryAddConnection(new AddConnectionRequest("connection-1", Guid.NewGuid()));
        _connectionRepository.TryRemoveConnection("connection-1", out _);

        bool result = _connectionRepository.ContainsConnection("connection-1");

        result.Should().BeFalse();
    }
}