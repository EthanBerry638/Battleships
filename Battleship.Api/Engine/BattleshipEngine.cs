using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Exceptions;

namespace Battleship.Api.Engine
{
    public class BattleshipEngine 
    {
        private readonly IGameBoard[] _gameBoards;
        private readonly Player[] _players;
        private readonly HashSet<Coordinate>[] _shotsTaken = [ [], [] ];
        private int _currentPlayerIndex;
        private GameState _gameState;
        public GameState GameState => _gameState;
        public Player CurrentPlayer => _players[_currentPlayerIndex];
        
        public BattleshipEngine(IGameBoard playerOneBoard, IGameBoard playerTwoBoard, Player playerOne, Player playerTwo)
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
            CheckGameState();
            if (_gameState == GameState.Finished) throw new GameOverException("Cannot shoot when game is over.");
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
        
        private void CheckGameState()
        {
            _gameState = _gameBoards[0].AreAllShipsSunk() || _gameBoards[1].AreAllShipsSunk() 
                ? GameState.Finished : GameState.Playing;
        }

        public GameStartResult TryStartGame()
        {
            if (_gameState == GameState.Playing) return GameStartResult.AlreadyStarted();
            
            var board1Check = _gameBoards[0].ValidateFleet();
            var board2Check = _gameBoards[1].ValidateFleet();

            if (!board1Check.IsValid || !board2Check.IsValid)
            {
                return GameStartResult.Invalid([board1Check, board2Check]);
            }
            
            _gameState = GameState.Playing;
            return GameStartResult.Ok();
        }

        public Player? GetWinner()
        {
            return null;
        }
    }
}
