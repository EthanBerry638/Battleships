using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities;

public interface IShip
{
    ShipType Type { get; }
    void RegisterHit(Coordinate coordinate);
    bool IsSunk();
    List<Coordinate> Coordinates { get; }
}