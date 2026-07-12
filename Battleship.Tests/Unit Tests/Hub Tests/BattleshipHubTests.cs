using Battleship.Api.Hubs;
using Battleship.Api.Services;
using Battleship.Api.Engine;
using Microsoft.AspNetCore.SignalR;
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

    [Theory]
    [InlineData("DOESNOTEXIST")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task JoinGame_ShouldReturnFalse_WhenGameDoesNotExist(string? gameCode)
    {
        _mockManager.Setup(m => m.GetGame(It.IsAny<string>()))
            .Returns((BattleshipEngine?)null);

        var result = await CreateHub().JoinGame(gameCode!);

        result.Should().BeFalse();
        
        _mockManager.Verify(m => m.GetGame(gameCode!), Times.Once);
    }
}