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
}