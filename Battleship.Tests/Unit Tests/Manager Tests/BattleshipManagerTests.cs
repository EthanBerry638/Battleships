using Battleship.Api.Services;
using FluentAssertions;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Tests.Unit_Tests.Manager_Tests;

public class GameManagerTests
{
    private readonly BattleshipManager _manager = new();
    private readonly Player _dummyPlayer1 = new("Player 1");
    private readonly Player _dummyPlayer2 = new("Player 2");

    [Fact]
    public void CreateGame_ShouldReturnSixCharacterCode_WhenCalled()
    {
        string result = _manager.CreateGame(_dummyPlayer1, _dummyPlayer2);
        
        result.Length.Should().Be(6);
        result.Should().NotBeNull();
    }
    
    [Fact]
    public void CreateGame_ShouldReturnUniqueCodes_WhenCalledMultipleTimes()
    {
        string code1 = _manager.CreateGame(_dummyPlayer1, _dummyPlayer2);
        string code2 = _manager.CreateGame(_dummyPlayer1, _dummyPlayer2);

        code1.Should().NotBe(code2);
    }
    
    [Fact]
    public void CreateGame_ShouldReturnUppercaseCode_WhenCalled()
    {
        string result = _manager.CreateGame(_dummyPlayer1, _dummyPlayer2);
        
        result.Should().BeUpperCased();
    }
    
    [Fact]
    public void GetGame_ShouldReturnEngineInstance_WhenGameExists()
    {
        string gameCode = _manager.CreateGame(_dummyPlayer1, _dummyPlayer2);
        
        var result = _manager.GetGame(gameCode);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetGame_ShouldReturnMultipleEngineInstances_WhenCalledMultipleTimesOnExistingGames()
    {
        for (int i = 0; i < 10; i++)
        {
            string gameCode = _manager.CreateGame(_dummyPlayer1, _dummyPlayer2);
            
            var result = _manager.GetGame(gameCode);
            
            result.Should().NotBeNull();
        }
    }
    
    [Theory]
    [InlineData("123456")]   
    [InlineData("ABCDEF")]  
    [InlineData("123-ABC")]  
    [InlineData("abcdef")]
    [InlineData("")]         
    [InlineData("   ")]      
    [InlineData(null)]       
    public void GetGame_ShouldReturnNull_WhenGameDoesNotExist(string? code)
    {
        var result = _manager.GetGame(code!);
        
        result.Should().BeNull();
    }
}