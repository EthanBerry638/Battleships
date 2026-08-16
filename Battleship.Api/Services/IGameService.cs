using Battleship.Api.GamePieces.Data;
using Battleship.Api.DTOs;
using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Responses;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Services;

public interface IGameService
{
    PlacementResult PlaceShip(PlaceShipRequest request);
    StartGameResponse TryStartGame(Guid playerId);
    Player? GetWinner(string gameCode);
    FleetValidationResult ValidateFleet(Guid playerId);
}