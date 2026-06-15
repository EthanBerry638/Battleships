using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities;

public interface IShip
{
    void RegisterHit(Coordinate coordinate);
    bool IsSunk();
}