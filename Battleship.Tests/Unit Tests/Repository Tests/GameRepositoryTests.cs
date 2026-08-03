using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.GamePieces.Board;  
using Battleship.Api.Repositories;
using Battleship.Api.Services;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Repository_Tests;

public class GameRepositoryTests
{
    private readonly GameRepository _gameRepository = new();
    private readonly Player _player1 = new(Guid.NewGuid(), "Player 1");
    private readonly Player _player2 = new(Guid.NewGuid(), "Player 2");
    
    private static GameSession CreateSession(Player playerOne, Player playerTwo)
    {
        BattleshipEngine engine = new(new GameBoard(), new GameBoard(), playerOne, playerTwo);
        return new GameSession(engine);
    }
    
    [Fact]
    public void TryAddGame_ShouldReturnTrue_WhenGameDoesNotExist()
    {
        bool result = _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));

        result.Should().BeTrue();
    }

    [Fact]
    public void TryAddGame_ShouldReturnFalse_WhenCodeAlreadyExists()
    {
        _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));

        bool result = _gameRepository.TryAddGame("ABC123", CreateSession(_player2, _player1));

        result.Should().BeFalse();
    }
    [Fact]
    public void TryRemoveGame_ShouldReturnTrue_WhenGameExists()
    {
        _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));

        bool result = _gameRepository.TryRemoveGame("ABC123");

        result.Should().BeTrue();
    }

    [Fact]
    public void TryRemoveGame_ShouldReturnFalse_WhenGameDoesNotExist()
    {
        bool result = _gameRepository.TryRemoveGame("ABC123");

        result.Should().BeFalse();
    }

    [Fact]
    public void TryRemoveGame_ShouldAllowCodeReuse_WhenGameHasBeenRemoved()
    {
        _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));
        _gameRepository.TryRemoveGame("ABC123");

        bool result = _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));

        result.Should().BeTrue();
    }
    
    [Fact]
    public void FindKeyByPlayerId_ShouldReturnTrueAndOutputCode_WhenPlayerIsInGame()
    {
        _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));

        bool result = _gameRepository.TryFindKeyByPlayerId(_player1.Id, out string? gameCode);

        result.Should().BeTrue();
        gameCode.Should().Be("ABC123");
    }

    [Fact]
    public void FindKeyByPlayerId_ShouldReturnFalse_WhenPlayerIsNotInAnyGame()
    {
        bool result = _gameRepository.TryFindKeyByPlayerId(_player1.Id, out string? gameCode);

        result.Should().BeFalse();
        gameCode.Should().BeNull();
    }

    [Fact]
    public void FindKeyByPlayerId_ShouldReturnCorrectCode_WhenMultipleGamesExist()
    {
        Player player3 = new(Guid.NewGuid(), "Player 3");
        Player player4 = new(Guid.NewGuid(), "Player 4");

        _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));
        _gameRepository.TryAddGame("XYZ789", CreateSession(player3, player4));

        bool result = _gameRepository.TryFindKeyByPlayerId(player4.Id, out string? gameCode);

        result.Should().BeTrue();
        gameCode.Should().Be("XYZ789");
    }
    
    [Fact]
    public void GetByCode_ShouldReturnTrueAndOutputSession_WhenGameExists()
    {
        GameSession session = CreateSession(_player1, _player2);
        _gameRepository.TryAddGame("ABC123", session);

        bool result = _gameRepository.TryGetGameByCode("ABC123", out GameSession? retrievedSession);

        result.Should().BeTrue();
        retrievedSession.Should().Be(session);
    }

    [Fact]
    public void GetByCode_ShouldReturnFalse_WhenGameDoesNotExist()
    {
        bool result = _gameRepository.TryGetGameByCode("ABC123", out GameSession? retrievedSession);

        result.Should().BeFalse();
        retrievedSession.Should().BeNull();
    }
    
    [Fact]
    public void IsPlayerInGame_ShouldReturnTrue_WhenPlayerIsInGame()
    {
        _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));

        bool result = _gameRepository.IsPlayerInGame(_player1.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsPlayerInGame_ShouldReturnFalse_WhenPlayerIsNotInAnyGame()
    {
        bool result = _gameRepository.IsPlayerInGame(_player1.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsPlayerInGame_ShouldReturnFalse_WhenPlayerHasBeenRemoved()
    {
        _gameRepository.TryAddGame("ABC123", CreateSession(_player1, _player2));
        _gameRepository.TryRemoveGame("ABC123");

        bool result = _gameRepository.IsPlayerInGame(_player1.Id);

        result.Should().BeFalse();
    }
}