using System.Collections.Concurrent;
using Battleship.Api.DTOs;

namespace Battleship.Api.Repositories;

public class ConnectionRepository : IConnectionRepository
{
    private readonly ConcurrentDictionary<string, Guid> _connections = new();

    public void SetConnection(string connectionId, Guid playerId)
    {
        _connections[connectionId] = playerId;
    }

    public bool TryGetPlayerId(string connectionId, out Guid playerId)
    {
        return _connections.TryGetValue(connectionId, out playerId);
    }

    public bool TryRemoveConnection(string connectionId, out Guid playerId)
    {
        return _connections.TryRemove(connectionId, out playerId);
    }
    
    public bool ContainsConnection(string connectionId)
    {
        return _connections.ContainsKey(connectionId);
    }
    
    public bool ContainsPlayer(Guid playerId)
    {
        return _connections.Values.Contains(playerId);
    }
}