using System.Runtime.InteropServices;
using Battleship.Api.Services;
using FluentAssertions;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Tests.Unit_Tests.Manager_Tests;

public class BattleshipManagerTests
{
    private readonly BattleshipManager _manager = new();
    private readonly Player _dummyPlayer1 = new(Guid.NewGuid(), "Player 1");
    private readonly Player _dummyPlayer2 = new(Guid.NewGuid(), "Player 2");

    [Fact]
    public void CreateLobby_ShouldReturnSixCharacterCode_WhenCalled()
    {
        string result = _manager.CreateLobby(_dummyPlayer1);

        result.Length.Should().Be(6);
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateLobby_ShouldReturnUniqueCodes_WhenCalledMultipleTimes()
    {
        string code1 = _manager.CreateLobby(_dummyPlayer1);
        string code2 = _manager.CreateLobby(_dummyPlayer1);

        code1.Should().NotBe(code2);
    }

    [Fact]
    public void CreateLobby_ShouldReturnUppercaseCode_WhenCalled()
    {
        string result = _manager.CreateLobby(_dummyPlayer1);

        result.Should().BeUpperCased();
    }

    [Fact]
    public void CreateLobby_ShouldThrowArgumentNullException_WhenPlayerIsNull()
    {
        var act = () => _manager.CreateLobby(null!);

        act.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void CreateLobby_ShouldRetryGeneration_WhenCodeCollisionOccurs()
    {
        var managerWithCollision = new CollidingBattleshipManager();
        var player1 = new Player(Guid.NewGuid(), "Original Player");
        managerWithCollision.CreateLobby(player1); 
        var player2 = new Player(Guid.NewGuid(), "Colliding Player");
        
        string result = managerWithCollision.CreateLobby(player2);
        
        result.Should().Be("UNIQUE");
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
    
    [Fact]
    public void GetGame_ShouldReturnTheActiveEngine_WhenGameHasBeenStartedViaJoinLobby()
    {
        string gameCode = _manager.CreateLobby(_dummyPlayer1);
        _manager.GetGame(gameCode).Should().BeNull();
        
        var createdEngine = _manager.JoinLobby(gameCode, _dummyPlayer2);
        var retrievedEngine = _manager.GetGame(gameCode);
        
        retrievedEngine.Should().NotBeNull();
        retrievedEngine.Should().Be(createdEngine); 
    }
    
    [Fact]
    public void JoinLobby_ShouldThrowArgumentNullException_WhenPlayer2IsNull()
    {
        var act = () => _manager.JoinLobby("ABC123", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void JoinLobby_ShouldReturnNull_WhenGameCodeIsInvalid(string? gameCode)
    {
        var result = _manager.JoinLobby(gameCode!, _dummyPlayer2);

        result.Should().BeNull();
    }
    
    [Fact]
    public void JoinLobby_ShouldCreateEngine_WhenLobbyExists()
    {
        string gameCode = _manager.CreateLobby(_dummyPlayer1);
        
        var engine = _manager.JoinLobby(gameCode, _dummyPlayer2);

        engine.Should().NotBeNull();
        engine!.CurrentPlayer.Should().Be(_dummyPlayer1);
    }
    
    [Theory]
    [InlineData("FAKE12")]
    [InlineData("NOTLBY")]
    [InlineData("000000")]
    [InlineData("ABC-12")]
    [InlineData("1")]
    [InlineData("INVALIDcode")]
    public void JoinLobby_ShouldReturnNull_WhenLobbyDoesNotExist(string gameCode)
    {
        var result = _manager.JoinLobby(gameCode, _dummyPlayer2);
        
        result.Should().BeNull();
    }
    
    [Fact]
    public void JoinLobby_ShouldOnlyAllowOneJoin_WhenCalledMultipleTimes()
    {
        string gameCode = _manager.CreateLobby(_dummyPlayer1);
        var player3 = new Player(Guid.NewGuid(), "Player 3");

        var firstJoin = _manager.JoinLobby(gameCode, _dummyPlayer2);
        var secondJoin = _manager.JoinLobby(gameCode, player3);

        firstJoin.Should().NotBeNull();
        secondJoin.Should().BeNull(); 
    }
}

public class CollidingBattleshipManager : BattleshipManager
{
    private int _callCount = 0;

    protected override string GenerateCode()
    {
        _callCount++;
        if (_callCount == 1) return "DUPLIC";
        if (_callCount == 2) return "DUPLIC";
        return "UNIQUE";
    }
}