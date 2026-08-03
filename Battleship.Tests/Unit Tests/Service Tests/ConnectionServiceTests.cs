using Battleship.Api.Services;
using Battleship.Api.Repositories;
using Moq;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Service_Tests;

public class ConnectionServiceTests
{
    private readonly ConnectionService _connectionService;

    public ConnectionServiceTests()
    {
        var connectionRepositoryMock = new Mock<IConnectionRepository>();
        _connectionService = new ConnectionService(connectionRepositoryMock.Object);
    }
}