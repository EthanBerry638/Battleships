using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities;

public interface IPlayer
{
    ShotResult Shoot(Coordinate coordinate);
}