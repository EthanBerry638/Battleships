using Battleship.Api.Repositories;
using Battleship.Api.DTOs.Responses;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Services;

public class GameService (IGameRepository gameRepository) : IGameService
{
    private readonly IGameRepository _gameRepository = gameRepository;
    
    public PlacementResult PlaceShip(Guid playerId, ShipType type, List<Coordinate> coordinates)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(playerId, out string? gameCode))
            throw new PlayerNotFoundException($"No active game found for player with id {playerId}.");

        if (!_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");

        IShip ship = new Ship(type, coordinates);

        lock (session!.Lock)
        {
            if (session.IsPlayerReady(playerId))
                throw new FleetLockedException("You can't place a ship after readying.");

            return session.Engine.PlaceShip(playerId, ship);
        }
    }
    
    public StartGameResponse TryStartGame(Guid playerId)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(playerId, out string? gameCode))
            throw new PlayerNotFoundException($"No active game found for player with id {playerId}.");

        if (!_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");
        
        lock (session!.Lock)
        {
            if (!session.BothPlayersReady)
                return new StartGameResponse(false);

            session.Engine.StartGame();
            
            var playerIds = session.Engine.Players.Select(p => p.Id).ToList();

            return new StartGameResponse(true, gameCode!, session.Engine.CurrentPlayer.Id, playerIds);
        }
    }

    public FleetValidationResult ValidateFleet(Guid playerId)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(playerId, out string? gameCode))
            throw new PlayerNotFoundException($"No active game found for player with id {playerId}.");

        if (!_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");
        
        lock (session!.Lock)
        {
            FleetValidationResult result = session.Engine.ValidateFleet(playerId);
            
            if (result.IsValid)
                session.SetPlayerReady(playerId);

            return result;
        }
    }

    public ShotResponse Shoot(Guid playerId, Coordinate coordinate)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(playerId, out string? gameCode))
            throw new PlayerNotFoundException($"No active game found for player with id {playerId}.");

        if (!_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");

        lock (session!.Lock)
        {
            Player player = session.Engine.Players.First(p => p.Id == playerId);
            
            ShotOutcome outcome = session.Engine.Shoot(player, coordinate);

            if (!outcome.IsGameOver)
                return new ShotResponse(outcome.Result, gameCode!, playerId, coordinate, outcome.SunkCoordinates,
                    outcome.IsGameOver);
            
            _gameRepository.TryRemoveGame(gameCode!);
            var playerIds = session.Engine.Players.Select(p => p.Id).ToList();

            return new ShotResponse(outcome.Result, gameCode!, playerId, coordinate, outcome.SunkCoordinates,
                outcome.IsGameOver, playerIds);
        }
    }

    public void ClearBoard(Guid playerId)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(playerId, out string? gameCode))
            throw new PlayerNotFoundException($"No active game found for player with id {playerId}.");

        if (!_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");
        
        lock (session!.Lock)
        {
            if (session.IsPlayerReady(playerId))
                throw new FleetLockedException("You can't clear the board after readying.");

            session.Engine.ClearBoard(playerId);
        }
    }
}