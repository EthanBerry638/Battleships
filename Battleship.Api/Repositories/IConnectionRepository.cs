using Battleship.Api.DTOs;

namespace Battleship.Api.Repositories;

public interface IConnectionRepository
{
    bool TryAddConnection(string connectionId, Guid playerId);
    bool TryRemoveConnection(string connectionId, out Guid playerId);
    bool ContainsConnection(string connectionId);
    bool ContainsPlayer(Guid playerId);
}