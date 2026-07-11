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
    public void CreateGame_ShouldReturnUniqueCodes_WhenCalledMultipleTimes()
    {
        var manager = new BattleshipManager();

        string code1 = manager.CreateGame();
        string code2 = manager.CreateGame();

        code1.Should().NotBe(code2);
    }
    
    [Fact]
    public void CreateGame_ShouldReturnUppercaseCode_WhenCalled()
    {
        var manager = new BattleshipManager();
        
        string result = manager.CreateGame();
        
        result.Should().BeUpperCased();
    }
}