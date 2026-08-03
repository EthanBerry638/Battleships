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
}