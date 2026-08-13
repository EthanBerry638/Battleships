using Battleship.Api.Repositories;
using Battleship.Api.DTOs;
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
            return session.Engine.PlaceShip(request.PlayerId, ship);
        }
    }
    
    public StartGameResponse TryStartGame(Guid playerId)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(playerId, out string? gameCode))
            throw new PlayerNotFoundException($"No active game found for player with id {playerId}.");

        if (!_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");

        // TODO: Refactor game start execution order.
        // Currently, calling session.Engine.TryStartGame() validates both boards.
        // If player 1 clicks ready while player 2 is still configuring their fleet,
        // Player 1 receives and invalid fleet error for players 2's board and state mutates early
        
        lock (session!.Lock)
        {
            session.SetPlayerReady(playerId);

            if (!session.BothPlayersReady)
                return new StartGameResponse(gameCode!, GameStartResult.WaitingForOpponent());

            return new StartGameResponse(gameCode!, session.Engine.TryStartGame());
        }
    }

    public Player? GetWinner(string gameCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameCode);

        if (!_gameRepository.TryGetGameByCode(gameCode, out GameSession? session))
            throw new GameNotFoundException($"Game by game code: {gameCode} not found.");

        lock (session!.Lock)
        {
            return session.Engine.GetWinner();
        }
    }
}