using Battleship.Api.Exceptions;
using Battleship.Api.Services;
using FluentAssertions;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.DTOs;

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
        string code2 = _manager.CreateLobby(_dummyPlayer2);

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
    
    [Fact]
    public void CreateOrJoinLobby_ShouldThrowPlayerAlreadyInSessionException_WhenPlayerAlreadyHasOpenLobby()
    {
        string gameCode = _manager.CreateLobby(_dummyPlayer1);
        
        var actCreate = () => _manager.CreateLobby(_dummyPlayer1);
        var actJoin = () => _manager.JoinLobby(gameCode, _dummyPlayer1);
        
        actCreate.Should().Throw<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
        actJoin.Should().Throw<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
    }

    [Fact]
    public void CreateOrJoinLobby_ShouldThrowPlayerAlreadyInSessionException_WhenPlayerAlreadyInActiveGame()
    {
        string gameCode1 = _manager.CreateLobby(_dummyPlayer1); 
        _manager.JoinLobby(gameCode1, _dummyPlayer2); 
        string gameCode2 = _manager.CreateLobby(new Player(Guid.NewGuid(), "Player 3"));
        
        var actCreate = () => _manager.CreateLobby(_dummyPlayer1);
        var actJoin = () => _manager.JoinLobby(gameCode2, _dummyPlayer1);
        
        actCreate.Should().Throw<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
        actJoin.Should().Throw<PlayerAlreadyInSessionException>()
            .WithMessage("Player is already in an active lobby or game.");
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

    [Fact]
    public void AddConnection_ShouldReturnTrue_WhenPassedValidConnectionIdAndGuid()
    {
        var request = new AddConnectionRequest("test-connection-123", Guid.NewGuid());
        
        var result = _manager.AddConnection(request);
        
        result.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidAddConnectionData))]
    public void AddConnection_ShouldThrowException_WhenPassedNullOrEmptyConnectionIdOrGuid(string connectionId,
        Guid guid)
    {
        var request = new AddConnectionRequest(connectionId, guid);
        
        var act = () => _manager.AddConnection(request);
        
        act.Should().Throw<ArgumentException>()
            .WithMessage("ConnectionId and/or Guid cannot be null or empty.");
    }
    
    public static IEnumerable<object[]> InvalidAddConnectionData =>
    [
        [null!, Guid.NewGuid()],
        ["", Guid.NewGuid()],
        [" ", Guid.NewGuid()],
        ["test-connection-123", Guid.Empty],
    ];
    
    [Fact]
    public void AddConnection_ShouldReturnFalse_WhenConnectionAlreadyExists()
    {
        var request = new AddConnectionRequest("test-connection-123", Guid.NewGuid());
        _manager.AddConnection(request);

        var result = _manager.AddConnection(request);

        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task HandleDisconnectAsync_ShouldRemoveConnectionImmediately_WhenConnectionIdExists()
    {
        string connectionId = "test-connection-123";
        var playerId = Guid.NewGuid();
        var request = new AddConnectionRequest(connectionId, playerId);
        _manager.AddConnection(request);

        await _manager.HandleDisconnectAsync(connectionId,TimeSpan.Zero);
        var canAddConnection = _manager.AddConnection(request);
        
        canAddConnection.Should().BeTrue();
    }
    
    [Fact]
    public async Task HandleDisconnectAsync_ShouldRemoveLobby_WhenPlayerDoesNotReconnectWithinDelay()
    {
        string connectionId = "host-connection-123";
        var player = new Player(Guid.NewGuid(), "Lobby Host");
        var dummyJoiner = new Player(Guid.NewGuid(), "Joiner");
        _manager.AddConnection(new AddConnectionRequest(connectionId, player.Id));
        string gameCode = _manager.CreateLobby(player);
        
        await _manager.HandleDisconnectAsync(connectionId, TimeSpan.Zero);
        var engine = _manager.JoinLobby(gameCode, dummyJoiner);
    
        engine.Should().BeNull();
    }
    
    [Fact]
    public async Task HandleDisconnectAsync_ShouldRemoveActiveGame_WhenPlayerDoesNotReconnectWithinDelay()
    {
        var connectionId = "player1-connection-123";
        _manager.AddConnection(new AddConnectionRequest(connectionId, _dummyPlayer1.Id));
        string gameCode = _manager.CreateLobby(_dummyPlayer1);
        _manager.JoinLobby(gameCode, _dummyPlayer2);
        
        await _manager.HandleDisconnectAsync(connectionId, TimeSpan.Zero);
        var activeGame = _manager.GetGame(gameCode);
        
        activeGame.Should().BeNull();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleDisconnectAsync_ShouldThrowArgumentException_WhenConnectionIdIsInvalid(string? invalidConnectionId)
    {
        var act = async () => await _manager.HandleDisconnectAsync(invalidConnectionId!, TimeSpan.Zero);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Connection ID cannot be null or whitespace");
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