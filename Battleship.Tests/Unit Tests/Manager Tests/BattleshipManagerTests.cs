using Battleship.Api.Services;
using FluentAssertions;

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
    
    [Fact]
    public void CreateGame_ShouldNotDuplicateCodes_WhenLargeVolumeGenerated()
    {
        var manager = new BattleshipManager();
        var codes = new HashSet<string>();
        
        for (int i = 0; i < 10000; i++)
        {
            string code = manager.CreateGame();
            codes.Add(code).Should().BeTrue();
        }
    }
}