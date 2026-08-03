using Battleship.Api.GamePieces.Entities;

namespace Battleship.Tests.Unit_Tests.Repository_Tests;

public class LobbyRepository
{
    private readonly LobbyRepository _lobbyRepository = new();
    private readonly Player _player1 = new(Guid.NewGuid(), "Player 1");
    private readonly Player _player2 = new(Guid.NewGuid(), "Player 2");
}