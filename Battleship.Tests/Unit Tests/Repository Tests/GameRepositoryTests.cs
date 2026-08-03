using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Repositories;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Repository_Tests;

public class GameRepositoryTests
{
    private readonly GameRepository _gameRepository = new();
    private readonly Player _player1 = new(Guid.NewGuid(), "Player 1");
    private readonly Player _player2 = new(Guid.NewGuid(), "Player 2");
}