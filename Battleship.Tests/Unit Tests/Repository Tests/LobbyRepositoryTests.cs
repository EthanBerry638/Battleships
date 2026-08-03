using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Repositories;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Repository_Tests;

public class LobbyRepositoryTests
{
    private readonly LobbyRepository _lobbyRepository = new();
    private readonly Player _player1 = new(Guid.NewGuid(), "Player 1");
    private readonly Player _player2 = new(Guid.NewGuid(), "Player 2");
    
    
    [Fact]
    public void TryAddLobby_ShouldReturnTrue_WhenLobbyDoesNotExist()
    {
        bool result = _lobbyRepository.TryAddLobby("ABC123", _player1);

        result.Should().BeTrue();
    }

    [Fact]
    public void TryAddLobby_ShouldReturnFalse_WhenCodeAlreadyExists()
    {
        _lobbyRepository.TryAddLobby("ABC123", _player1);

        bool result = _lobbyRepository.TryAddLobby("ABC123", _player2);

        result.Should().BeFalse();
    }
    
    [Fact]
    public void TryRemoveLobby_ShouldReturnTrueAndOutputPlayer_WhenLobbyExists()
    {
        _lobbyRepository.TryAddLobby("ABC123", _player1);

        bool result = _lobbyRepository.TryRemoveLobby("ABC123", out Player? removedPlayer);

        result.Should().BeTrue();
        removedPlayer.Should().Be(_player1);
    }

    [Fact]
    public void TryRemoveLobby_ShouldReturnFalse_WhenLobbyDoesNotExist()
    {
        bool result = _lobbyRepository.TryRemoveLobby("ABC123", out Player? removedPlayer);

        result.Should().BeFalse();
        removedPlayer.Should().BeNull();
    }

    [Fact]
    public void TryRemoveLobby_ShouldAllowCodeReuse_WhenLobbyHasBeenRemoved()
    {
        _lobbyRepository.TryAddLobby("ABC123", _player1);
        _lobbyRepository.TryRemoveLobby("ABC123", out _);

        bool result = _lobbyRepository.TryAddLobby("ABC123", _player2);

        result.Should().BeTrue();
    }
    
    [Fact]
    public void TryFindCodeByPlayer_ShouldReturnTrueAndOutputCode_WhenPlayerIsInLobby()
    {
        _lobbyRepository.TryAddLobby("ABC123", _player1);

        bool result = _lobbyRepository.TryFindCodeByPlayer(_player1.Id, out string? gameCode);

        result.Should().BeTrue();
        gameCode.Should().Be("ABC123");
    }

    [Fact]
    public void TryFindCodeByPlayer_ShouldReturnFalse_WhenPlayerIsNotInAnyLobby()
    {
        bool result = _lobbyRepository.TryFindCodeByPlayer(_player1.Id, out string? gameCode);

        result.Should().BeFalse();
        gameCode.Should().BeNull();
    }

    [Fact]
    public void TryFindCodeByPlayer_ShouldReturnCorrectCode_WhenMultipleLobbiesExist()
    {
        _lobbyRepository.TryAddLobby("ABC123", _player1);
        _lobbyRepository.TryAddLobby("XYZ789", _player2);

        bool result = _lobbyRepository.TryFindCodeByPlayer(_player2.Id, out string? gameCode);

        result.Should().BeTrue();
        gameCode.Should().Be("XYZ789");
    }
}