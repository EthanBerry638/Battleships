using Battleship.Api.DTOs;

namespace Battleship.Api.Services;

public interface IConnectionService
{
    bool AddConnection(string connectionId, Guid playerId);
    Task<string?> HandleDisconnectAsync(string connectionId, TimeSpan delay = default);
}