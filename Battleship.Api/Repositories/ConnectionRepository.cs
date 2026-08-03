using System.Collections.Concurrent;

namespace Battleship.Api.Repositories;

public class ConnectionRepository
{
    private readonly ConcurrentDictionary<string, Guid> _connections = new();
}