using Battleship.Api.Services;
using FluentAssertions;
using Xunit;

namespace Battleship.Tests.Unit_Tests.Manager_Tests;

public class GameManagerTests
{
    [Fact]
    public void CreateGame_ShouldReturnSixCharacterCode_WhenCalled()
    {
        var manager = new BattleshipManager();
        
        string result = manager.CreateGame();
        
        result.Length.Should().Be(6);
        result.Should().NotBeNull();
    }
}