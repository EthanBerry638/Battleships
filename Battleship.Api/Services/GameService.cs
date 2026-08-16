using Battleship.Api.Repositories;
using Battleship.Api.DTOs.Requests;
using Battleship.Api.DTOs.Responses;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Services;

public class GameService (IGameRepository gameRepository) : IGameService
{
    private readonly IGameRepository _gameRepository = gameRepository;
    
    public PlacementResult PlaceShip(PlaceShipRequest request)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(request.PlayerId, out string? gameCode))
            throw new PlayerNotFoundException($"No active game found for player with id {request.PlayerId}.");
            
        if (!_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");

        IShip ship = new Ship(request.Type, request.Coordinates);
        
        lock (session!.Lock)
        {
            if (session.IsPlayerReady(request.PlayerId))
                throw new FleetLockedException("You can't place a ship after readying.");
            
            return session.Engine.PlaceShip(request.PlayerId, ship);
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

    public Player? GetWinner(string gameCode)
    {
        if (!_gameRepository.TryGetGameByCode(gameCode, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");

        lock (session!.Lock)
        {
            return session.Engine.GetWinner();
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
}