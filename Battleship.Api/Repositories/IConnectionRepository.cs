using Battleship.Api.DTOs;

namespace Battleship.Api.Repositories;

public interface IConnectionRepository
{
    bool TryAddConnection(AddConnectionRequest request);
    bool TryRemoveConnection(string connectionId, out Guid playerId);
    bool ContainsConnection(string connectionId);
}