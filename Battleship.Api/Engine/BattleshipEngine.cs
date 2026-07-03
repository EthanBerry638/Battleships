using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Engine
{
    public class BattleshipEngine 
    {
        private readonly IGameBoard[] _gameBoards;
        private readonly IPlayer[] _players;
        private readonly HashSet<Coordinate>[] _shotsTaken = [ [], [] ];
        private int _currentPlayerIndex;
        
        public BattleshipEngine(IGameBoard playerOneBoard, IGameBoard playerTwoBoard, IPlayer playerOne, IPlayer playerTwo)
        {
            ArgumentNullException.ThrowIfNull(playerOneBoard);
            ArgumentNullException.ThrowIfNull(playerTwoBoard);
            ArgumentNullException.ThrowIfNull(playerOne);
            ArgumentNullException.ThrowIfNull(playerTwo);
            
            _gameBoards = [playerOneBoard, playerTwoBoard];
            _players = [playerOne, playerTwo];
        }
        
        public ShotResult Shoot(Coordinate coordinate)
        {
            var opponentBoardIndex = (_currentPlayerIndex + 1) % 2;
            ShotResult shotResult;
            
            if (!_shotsTaken[_currentPlayerIndex].Add(coordinate))
            {
                return ShotResult.Duplicate;
            }

            var tile = _gameBoards[opponentBoardIndex].GetTile(coordinate);

            if (tile.HasShip)
            {
                var ship = tile.OccupyingShip!;

                ship.RegisterHit(coordinate);

                shotResult = ship.IsSunk() ? ShotResult.Sunk : ShotResult.Hit;
            }
            else
            {
                shotResult = ShotResult.Miss;
            }
            
            SwitchTurns();
            return shotResult;
        }

        private void SwitchTurns()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % 2;
        }
    }
}
