using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.Engine
{
    public class BattleshipEngine (IGameBoard gameBoard)
    {
        private readonly IGameBoard _gameBoard = gameBoard;
        
        public ShotResult Shoot(Coordinate coordinate)
        {
            _gameBoard.GetTile(coordinate);
            return ShotResult.Hit;
        }
    }
}
