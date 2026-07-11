using Battleship.Api.Services;
using Battleship.Api.Engine;
using FluentAssertions;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Entities;
using Moq;

namespace Battleship.Tests.Unit_Tests.Manager_Tests;

public class GameManagerTests
{
    private readonly BattleshipManager _manager;

    public GameManagerTests()
    {
        var mockFactory = new Mock<IBattleshipEngineFactory>();
        
        var mockEngine = new Mock<BattleshipEngine>(
            new Mock<IGameBoard>().Object, 
            new Mock<IGameBoard>().Object, 
            new Mock<Player>("Player 1").Object, 
            new Mock<Player>("Player 2").Object
        );

        mockFactory.Setup(f => f.Create()).Returns(mockEngine.Object);

        _manager = new BattleshipManager(mockFactory.Object);
    }

    [Fact]
    public void CreateGame_ShouldReturnSixCharacterCode_WhenCalled()
    {
        string result = _manager.CreateGame();
        
        result.Length.Should().Be(6);
        result.Should().NotBeNull();
    }
    
    [Fact]
    public void CreateGame_ShouldReturnUniqueCodes_WhenCalledMultipleTimes()
    {
        string code1 = _manager.CreateGame();
        string code2 = _manager.CreateGame();

        code1.Should().NotBe(code2);
    }
    
    [Fact]
    public void CreateGame_ShouldReturnUppercaseCode_WhenCalled()
    {
        string result = _manager.CreateGame();
        
        result.Should().BeUpperCased();
    }
    
    [Fact]
    public void GetGame_ShouldReturnEngineInstance_WhenGameExists()
    {
        string gameCode = _manager.CreateGame();
        
        var result = _manager.GetGame(gameCode);
        
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetGame_ShouldReturnMultipleEngineInstances_WhenCalledMultipleTimesOnExistingGames()
    {
        for (int i = 0; i < 10; i++)
        {
            string gameCode = _manager.CreateGame();
            
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