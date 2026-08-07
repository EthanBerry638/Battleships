using Battleship.Api.Repositories;
using Battleship.Api.DTOs;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.Services;

public class GameService (IGameRepository gameRepository)
{
    private readonly IGameRepository _gameRepository = gameRepository;
    
    public PlacementResult PlaceShip(PlaceShipRequest request)
    {
        if (!_gameRepository.TryFindKeyByPlayerId(request.PlayerId, out string? gameCode) ||
            !_gameRepository.TryGetGameByCode(gameCode!, out GameSession? session))
            throw new PlayerNotFoundException($"No active game found for player with id {request.PlayerId}.");

        lock (session!.Lock)
        {
            return session.Engine.PlaceShip(request);
        }
    }
}