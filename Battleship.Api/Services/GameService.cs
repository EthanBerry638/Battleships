using Battleship.Api.Repositories;
using Battleship.Api.DTOs;
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
            return session.Engine.PlaceShip(request.PlayerId, ship);
        }
    }
    
    public StartGameOutcome TryStartGame(Guid playerId)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(playerId, out string? gameCode))
            throw new PlayerNotFoundException($"No active game found for player with id {playerId}.");

        if (!_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");

        lock (session!.Lock)
        {
            session.SetPlayerReady(playerId);

            if (!session.BothPlayersReady)
                return new StartGameOutcome(gameCode, GameStartResult.WaitingForOpponent());

            return new StartGameOutcome(gameCode, session.Engine.TryStartGame());
        }
    }
}