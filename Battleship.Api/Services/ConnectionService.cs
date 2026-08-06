using Battleship.Api.Repositories;
using Battleship.Api.DTOs;

namespace Battleship.Api.Services;

public class ConnectionService (IConnectionRepository connectionRepository, IGameRepository gameRepository, ILobbyRepository lobbyRepository )
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    
    public bool AddConnection(AddConnectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ConnectionId) || request.PlayerId == Guid.Empty)
            throw new ArgumentException("ConnectionId and/or Guid cannot be null or empty.");

        return _connectionRepository.TryAddConnection(request);
    }
    
    public async Task<string?> HandleDisconnectAsync(string connectionId, TimeSpan delay = default)
    {
        throw new NotImplementedException();
    }
}