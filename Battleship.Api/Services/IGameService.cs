using Battleship.Api.GamePieces.Data;
using Battleship.Api.DTOs;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Services;

public interface IGameService
{
    PlacementResult PlaceShip(PlaceShipRequest request);
    StartGameOutcome TryStartGame(Guid playerId);
    Player? GetWinner(string gameCode);
}