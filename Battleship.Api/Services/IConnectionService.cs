using Battleship.Api.DTOs;

namespace Battleship.Api.Services;

public interface IConnectionService
{
    bool AddConnection(AddConnectionRequest request);
    Task<string?> HandleDisconnectAsync(string connectionId, TimeSpan delay = default);
}