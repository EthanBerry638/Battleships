using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Services;

namespace Battleship.Api.Repositories;

public interface IGameRepository
{
    bool TryAddGame(string gameCode, GameSession session);
    bool TryRemoveGame(string gameCode);
    bool FindKeyByPlayerId(Guid playerId, out string? gameCode);
    bool GetByCode(string gameCode, out GameSession? session);
    bool IsPlayerInGame(Guid playerId);
}