using Battleship.Api.Repositories;
using Battleship.Api.DTOs;

namespace Battleship.Api.Services;

public class ConnectionService (IConnectionRepository connectionRepository, IGameRepository gameRepository, ILobbyRepository lobbyRepository) : IConnectionService
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    
    public bool AddConnection(string connectionId, Guid playerId)
    {
        return _connectionRepository.TryAddConnection(connectionId, playerId);
    }
    
    public async Task<string?> HandleDisconnectAsync(string connectionId, TimeSpan delay = default)
    {
        if (!_connectionRepository.TryRemoveConnection(connectionId, out Guid playerId))
            return null;

        await Task.Delay(delay);

        if (_connectionRepository.ContainsPlayer(playerId))
            return null;

        if (_lobbyRepository.TryFindCodeByPlayer(playerId, out string? lobbyCode) && lobbyCode is not null)
        {
            _lobbyRepository.TryRemoveLobby(lobbyCode, out _);
            return null;
        }

        if (!_gameRepository.TryFindKeyByPlayerId(playerId, out string? gameCode) || gameCode is null)
            return null;

        _gameRepository.TryRemoveGame(gameCode);
        return gameCode;
    }
}