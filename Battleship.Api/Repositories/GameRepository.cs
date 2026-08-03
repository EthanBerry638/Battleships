using System.Collections.Concurrent;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Services;

namespace Battleship.Api.Repositories;

public class GameRepository : IGameRepository
{
    private readonly ConcurrentDictionary<string, GameSession> _games = new();

    public bool TryAddGame(string gameCode, Player player)
    {
        throw new NotImplementedException();
    }
    
    public bool TryRemoveGame(string gameCode, out Player? player)
    {
        throw new NotImplementedException();
    }
    
    public bool TryFindKeyByPlayer(Guid playerId, out string? gameCode)
    {
        throw new NotImplementedException();
    }
    
    public bool GetGameByCode(string gameCode, out GameSession? game)
    {
        throw new NotImplementedException();
    }   
    
    public bool IsPlayerInGame(Guid playerId)
    {
        throw new NotImplementedException();
    }
}