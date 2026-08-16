using Battleship.Api.DTOs;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Exceptions;

namespace Battleship.Api.Engine;

public class BattleshipEngine(IGameBoard playerOneBoard, IGameBoard playerTwoBoard, Player playerOne, Player playerTwo)
{
    private readonly IGameBoard[] _gameBoards = [playerOneBoard, playerTwoBoard];
    private readonly Player[] _players = [playerOne, playerTwo];
    private readonly HashSet<Coordinate>[] _shotsTaken = [[], []];
    private int _currentPlayerIndex;
    private GameState _gameState;
    private Player? _winner;
    public GameState GameState => _gameState;
    public Player CurrentPlayer => _players[_currentPlayerIndex];
    public IReadOnlyList<Player> Players => _players;

    public ShotResult Shoot(Player player, Coordinate coordinate)
    {
        ValidateShotPreconditions(player);

        ShotResult shotResult = GetShotResult(coordinate);

        CheckGameState();

        if (_gameState is not GameState.Finished) SwitchTurns();

        return shotResult;
    }

    private void ValidateShotPreconditions(Player player)
    {
        if (player != CurrentPlayer)
            throw new NotYourTurnException("Cannot shoot when it is not your turn.");

        if (_gameState is GameState.Setup)
            throw new GameNotStartedException("Cannot shoot when game is not started.");

        if (_gameState is GameState.Finished)
            throw new GameOverException("Cannot shoot when game is over.");
    }

    private ShotResult GetShotResult(Coordinate coordinate)
    {
        if (!_shotsTaken[_currentPlayerIndex].Add(coordinate)) return ShotResult.Duplicate;

        int opponentIndex = (_currentPlayerIndex + 1) % 2;
        IGameBoard opponentBoard = _gameBoards[opponentIndex];

        Tile tile = opponentBoard.GetTile(coordinate);

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
        if (_gameState is GameState.Setup) return;

        if (_gameBoards[0].AreAllShipsSunk())
        {
            _gameState = GameState.Finished;
            _winner = _players[1];
        }
        else if (_gameBoards[1].AreAllShipsSunk())
        {
            _gameState = GameState.Finished;
            _winner = _players[0];
        }
    }

    public void StartGame()
    {
        if (_gameState is not GameState.Setup)
            throw new GameNotInSetupException("You can't start a game when it's already in progress/finished.");

        _gameState = GameState.Playing;
    }

    public Player? GetWinner()
    {
        CheckGameState();

        return _gameState is not GameState.Finished ? null : _winner;
    }

    public PlacementResult PlaceShip(Guid playerId, IShip ship)
    {
        if (_gameState is GameState.Finished)
            throw new GameOverException("You can't place a ship after the game is finished");

        if (_gameState is GameState.Playing)
            throw new GameInProgressException("You can't place a ship after the game has started");

        int playerIndex = Array.FindIndex(_players, p => p.Id == playerId);

        return _gameBoards[playerIndex].PlaceShip(ship);
    }

    public FleetValidationResult ValidateFleet(Guid playerId)
    {
        if (_gameState is not GameState.Setup)
            throw new GameNotInSetupException("You can't validate a fleet when you're not in the setup phase.");

        int playerIndex = Array.FindIndex(_players, p => p.Id == playerId); 
        
        return _gameBoards[playerIndex].ValidateFleet();
    }
}