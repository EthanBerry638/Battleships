using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Exceptions;

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
            if (_gameBoards[0].AreAllShipsSunk())
            {
                throw new GameOverException($"Cannot shoot. Player 2 has sunk all of Player 1's ships.");
            }
            
            if (_gameBoards[1].AreAllShipsSunk())
            {
                throw new GameOverException($"Cannot shoot. Player 1 has sunk all of Player 2's ships.");
            }
            
            var shotResult = GetShotResult(coordinate);
            SwitchTurns();
            return shotResult;
        }

        private ShotResult GetShotResult(Coordinate coordinate)
        {
            if (!_shotsTaken[_currentPlayerIndex].Add(coordinate))
            {
                return ShotResult.Duplicate;
            }
            
            var opponentIndex = (_currentPlayerIndex + 1) % 2;
            var opponentBoard = _gameBoards[opponentIndex];
            
            var tile = opponentBoard.GetTile(coordinate);

            if (!tile.HasShip) return ShotResult.Miss;
            
            tile.OccupyingShip!.RegisterHit(coordinate);
            return tile.OccupyingShip.IsSunk() ? ShotResult.Sunk : ShotResult.Hit;
        }
        
        private void SwitchTurns()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % 2;
        }
    }
}
