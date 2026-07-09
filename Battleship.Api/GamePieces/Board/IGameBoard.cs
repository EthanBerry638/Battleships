using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.GamePieces.Board;

public interface IGameBoard
{
    Tile GetTile(Coordinate coordinate);
    PlacementResult PlaceShip(IShip ship);
    bool AreAllShipsSunk();     
    FleetValidationResult ValidateFleet();
}