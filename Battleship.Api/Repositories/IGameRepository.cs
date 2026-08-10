using Battleship.Api.Engine;

namespace Battleship.Api.Repositories;

public interface IGameRepository
{
    bool TryAddGame(string gameCode, GameSession session);
    bool TryRemoveGame(string gameCode);
    bool TryFindKeyByPlayerId(Guid playerId, out string? gameCode);
    bool TryGetGameByCode(string gameCode, out GameSession? session);
    bool IsPlayerInGame(Guid playerId);
}