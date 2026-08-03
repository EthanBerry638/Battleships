using System.Collections.Concurrent;
using Battleship.Api.DTOs;

namespace Battleship.Api.Repositories;

public class ConnectionRepository
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
}