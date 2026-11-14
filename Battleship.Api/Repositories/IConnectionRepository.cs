using Battleship.Api.DTOs;

namespace Battleship.Api.Repositories;

public interface IConnectionRepository
{
    void SetConnection(string connectionId, Guid playerId);
    bool TryGetPlayerId(string connectionId, out Guid playerId);
    bool TryRemoveConnection(string connectionId, out Guid playerId);
    bool ContainsConnection(string connectionId);
    bool ContainsPlayer(Guid playerId);
    IReadOnlyList<string> GetConnectionIds(Guid playerId);
}