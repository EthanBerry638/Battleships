using Battleship.Api.DTOs;

namespace Battleship.Api.Services;

public interface IConnectionService
{
    void EnsureConnectionIsFree(string connectionId);
    void RegisterConnection(string connectionId, Guid playerId);
    Guid GetPlayerId(string connectionId);
    Task<string?> HandleDisconnectAsync(string connectionId, TimeSpan delay = default);
}