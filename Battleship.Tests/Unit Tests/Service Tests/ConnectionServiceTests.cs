using Battleship.Api.Services;
using Battleship.Api.DTOs;
using Battleship.Api.Repositories;
using Moq;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Service_Tests;

public class ConnectionServiceTests
{
    private readonly Mock<IConnectionRepository> _connectionRepositoryMock = new();
    private readonly ConnectionService _connectionService;

    public ConnectionServiceTests()
    {
        _connectionService = new ConnectionService(_connectionRepositoryMock.Object);
    }

    [Fact]
    public void AddConnection_ShouldReturnTrue_WhenConnectionIsValid()
    {
        var request = new AddConnectionRequest("conn-123", Guid.NewGuid());
        _connectionRepositoryMock.Setup(r => r.TryAddConnection(request)).Returns(true);

        bool result = _connectionService.AddConnection(request);
        
        result.Should().BeTrue();
        _connectionRepositoryMock.Verify(r => r.TryAddConnection(request), Times.Once);
    }

    [Fact]
    public void AddConnection_ShouldReturnFalse_WhenConnectionAlreadyExists()
    {
        var request = new AddConnectionRequest("conn-123", Guid.NewGuid());
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
        var request = new AddConnectionRequest("conn-123", Guid.Empty);

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
}