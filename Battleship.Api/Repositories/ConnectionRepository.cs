using System.Collections.Concurrent;
using Battleship.Api.DTOs;

namespace Battleship.Api.Repositories;

public class ConnectionRepository : IConnectionRepository
{
    private readonly ConcurrentDictionary<string, Guid> _connections = new();

    public bool TryAddConnection(string connectionId, Guid playerId)
    {
        return _connections.TryAdd(connectionId, playerId);
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