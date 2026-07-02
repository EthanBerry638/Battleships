using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Engine
{
    public class BattleshipEngine (IGameBoard playerOneBoard, IGameBoard playerTwoBoard, IPlayer playerOne, IPlayer playerTwo)
    {
        private readonly IGameBoard[] _gameBoards = [playerOneBoard, playerTwoBoard];
        private readonly IPlayer[] _players = [playerOne, playerTwo];
        private readonly HashSet<Coordinate> _shotsTaken = [];
        private int _currentPlayerIndex;
        private bool _isGameOver = false;
        
        public ShotResult Shoot(Coordinate coordinate)
        {
            var opponentBoardIndex = (_currentPlayerIndex + 1) % 2;
            
            if (!_shotsTaken.Add(coordinate))
            {
                return ShotResult.Duplicate;
            }

            var tile = _gameBoards[opponentBoardIndex].GetTile(coordinate);

            if (tile.HasShip)
            {
                var ship = tile.OccupyingShip!;
                
                ship.RegisterHit(coordinate);
                SwitchTurns();
                
                return ship.IsSunk() ? ShotResult.Sunk : ShotResult.Hit;
            }
            
            SwitchTurns();
            return ShotResult.Miss;
        }

        private void SwitchTurns()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % 2;
        }
    }
}
