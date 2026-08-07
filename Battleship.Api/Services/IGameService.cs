using Battleship.Api.GamePieces.Data;
using Battleship.Api.DTOs;

namespace Battleship.Api.Services;

public interface IGameService
{
    PlacementResult PlaceShip(PlaceShipRequest request);
}