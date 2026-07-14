using Battleship.Api.Hubs;
using Battleship.Api.Services;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Entities;
using Microsoft.AspNetCore.SignalR;
using Battleship.Api.DTOs;
using FluentAssertions;
using Moq;

namespace Battleship.Tests.Unit_Tests.Hub_Tests;

public class BattleshipHubTests
{
    private readonly Mock<IBattleshipManager> _mockManager = new();
    private readonly Mock<IGroupManager> _mockGroups = new();
    private readonly Mock<HubCallerContext> _mockContext = new();

    private BattleshipHub CreateHub() => new(_mockManager.Object)
    {
        Groups = _mockGroups.Object,
        Context = _mockContext.Object
    };
    
    private static BattleshipEngine CreateEngine() => new(
        new GameBoard(),
        new GameBoard(),
        new Player(Guid.Empty, "Player 1"), 
        new Player(Guid.Empty, "Player 2")
        );

    [Theory]
    [InlineData("DOESNOTEXIST")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task JoinGame_ShouldReturnFalseAndNotAddUserToGroup_WhenGameDoesNotExist(string? gameCode)
    {
        _mockManager.Setup(m => m.GetGame(It.IsAny<string>()))
            .Returns((BattleshipEngine?)null);

        var result = await CreateHub().JoinGame(gameCode!);

        result.Should().BeFalse();
        
        _mockManager.Verify(m => m.GetGame(gameCode!), Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Never);
        _mockGroups.Verify(g => g.AddToGroupAsync(
            It.IsAny<string>(),
             It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Theory]
    [InlineData("ABC123")]
    [InlineData("XYZ789")]
    [InlineData("123ABC")]
    public async Task JoinGame_ShouldReturnTrueAndAddUserToGroup_WhenGameExists(string gameCode)
    {
        _mockManager.Setup(m => m.GetGame(gameCode))
            .Returns(CreateEngine());
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockGroups
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateHub().JoinGame(gameCode);

        result.Should().BeTrue();
        _mockManager.Verify(m => m.GetGame(gameCode), Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync(
            "test-connection-id", gameCode, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task CreateGame_ShouldReturnGameCodeAndAddCallerToGroup_WhenCalled()
    {
        var request = new CreateLobbyRequest(Guid.Empty, "Player 1");
        _mockManager.Setup(m => m.CreateLobby(It.IsAny<Player>()))
            .Returns("ABC123");
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockGroups
            .Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CreateHub().CreateLobby(request);

        result.Should().Be("ABC123");
        _mockManager.Verify(m => m.CreateLobby(It.IsAny<Player>()), Times.Once);
        _mockContext.Verify(c => c.ConnectionId, Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync(
            "test-connection-id", "ABC123", It.IsAny<CancellationToken>()), Times.Once);
    }
}