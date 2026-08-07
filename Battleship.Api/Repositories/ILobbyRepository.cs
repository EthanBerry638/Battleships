using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Repositories;

public interface ILobbyRepository
{
    bool TryAddLobby(string gameCode, Player player);
    bool TryRemoveLobby(string gameCode, out Player? player);
    bool TryFindCodeByPlayer(Guid playerId, out string? gameCode);
    bool IsPlayerInLobby(Guid playerId);
}