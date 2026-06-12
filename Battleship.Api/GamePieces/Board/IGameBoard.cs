using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Board;

public interface IGameBoard
{
    Tile GetTile(Coordinate coordinate);
}