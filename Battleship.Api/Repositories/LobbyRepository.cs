using System.Collections.Concurrent;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Repositories;

public class LobbyRepository : ILobbyRepository
{
    private readonly ConcurrentDictionary<string, Player> _lobbies = new ();

    public bool TryAddLobby(string gameCode, Player player)
    {
        return _lobbies.TryAdd(gameCode, player);
    }

    public bool TryRemoveLobby(string gameCode, out Player player)
    {
        return _lobbies.TryRemove(gameCode, out player);
    }

    public bool TryFindCodeByPlayer(Guid playerId, out string gameCode)
    {
        throw new NotImplementedException();
    }

    public bool IsPlayerInLobby(Guid playerId)
    {
        throw new NotImplementedException();
    }
}