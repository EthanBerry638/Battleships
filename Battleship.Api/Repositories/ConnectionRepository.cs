using System.Collections.Concurrent;
using Battleship.Api.DTOs;

namespace Battleship.Api.Repositories;

public class ConnectionRepository : IConnectionRepository
{
    private readonly ConcurrentDictionary<string, Guid> _connections = new();

    public bool TryAddConnection(AddConnectionRequest request)
    {
        return _connections.TryAdd(request.ConnectionId, request.PlayerId);
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