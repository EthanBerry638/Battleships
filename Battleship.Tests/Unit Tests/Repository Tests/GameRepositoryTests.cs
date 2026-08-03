using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Repositories;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Repository_Tests;

public class GameRepositoryTests
{
    private readonly GameRepository _gameRepository = new();
    private readonly Player _player1 = new(Guid.NewGuid(), "Player 1");
    private readonly Player _player2 = new(Guid.NewGuid(), "Player 2");
    
    [Fact]
    public void TryAddGame_ShouldReturnTrue_WhenGameDoesNotExist()
    {
        bool result = _gameRepository.TryAddGame("ABC123", _player1);

        result.Should().BeTrue();
    }

    [Fact]
    public void TryAddGame_ShouldReturnFalse_WhenCodeAlreadyExists()
    {
        _gameRepository.TryAddGame("ABC123", _player1);

        bool result = _gameRepository.TryAddGame("ABC123", _player2);

        result.Should().BeFalse();
    }
}