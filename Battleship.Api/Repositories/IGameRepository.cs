using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Services;

namespace Battleship.Api.Repositories;

public interface IGameRepository
{
    bool TryAddGame(string gameCode, Player player);
    bool TryRemoveGame(string gameCode, out Player? player);
    bool TryFindKeyByPlayer(Guid playerId, out string? gameCode);
    bool GetGameByCode(string gameCode, out GameSession? game);
    bool IsPlayerInGame(Guid playerId);
}