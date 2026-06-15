using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.Engine
{
    public class BattleshipEngine (IGameBoard gameBoard)
    {
        private readonly IGameBoard _gameBoard = gameBoard;
        private readonly HashSet<ShotHistory> _shotsTaken = [];
        
        public ShotResult Shoot(Coordinate coordinate)
        { 
            var tile = _gameBoard.GetTile(coordinate);

            if (tile.HasShip)
            {
                var ship = tile.OccupyingShip!;
                
                ship.RegisterHit(coordinate);

                return ship.IsSunk() ? ShotResult.Sunk : ShotResult.Hit;
            }
            
            return ShotResult.Miss;
        }
    }
}
