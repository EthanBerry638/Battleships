using Battleship.Api.Repositories;
using Battleship.Api.DTOs;
using Battleship.Api.Exceptions;

namespace Battleship.Api.Services;

public class ConnectionService (IConnectionRepository connectionRepository, IGameRepository gameRepository, ILobbyRepository lobbyRepository) : IConnectionService
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    
    public void EnsureConnectionIsFree(string connectionId)
    {
        if (!_connectionRepository.TryGetPlayerId(connectionId, out Guid playerId))
            return;

        if (_lobbyRepository.IsPlayerInLobby(playerId) || _gameRepository.IsPlayerInGame(playerId))
            throw new PlayerAlreadyInSessionException("This connection is already in a lobby or game.");
    }

    public void RegisterConnection(string connectionId, Guid playerId)
    {
        _connectionRepository.SetConnection(connectionId, playerId);
    }

    public Guid GetPlayerId(string connectionId)
    {
        if (!_connectionRepository.TryGetPlayerId(connectionId, out Guid playerId))
            throw new PlayerNotFoundException("No player is registered for this connection.");

        return playerId;
    }

    public IReadOnlyList<string> GetConnectionIds(IEnumerable<Guid> playerIds)
    {
        return playerIds
            .SelectMany(_connectionRepository.GetConnectionIds)
            .ToList();
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