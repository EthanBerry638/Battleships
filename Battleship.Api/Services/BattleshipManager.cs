using System.Collections.Concurrent;
using Battleship.Api.DTOs;
using Battleship.Api.Engine;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.GamePieces.Board;

namespace Battleship.Api.Services;

public class BattleshipManager : IBattleshipManager
{
    private readonly ConcurrentDictionary<string, BattleshipEngine> _games = new();
    private readonly ConcurrentDictionary<string, Player> _lobbies = new();
    private readonly ConcurrentDictionary<string, Guid> _connections = new();

    public string CreateLobby(Player player1)
    {
        ArgumentNullException.ThrowIfNull(player1);
        CheckLobbyAndGame(player1);
        
        string gameCode;
        do
        {
            gameCode = GenerateCode();
        } while (!_lobbies.TryAdd(gameCode, player1));
    
        return gameCode;
    }

    private void CheckLobbyAndGame(Player player)
    {
        bool isWaitingInLobby = _lobbies.Values
            .Any(p => p.Id == player.Id);
        bool isPlayingInGame = _games.Values
            .Any(g => g.Players.Any(p => p.Id == player.Id));

        if (isWaitingInLobby || isPlayingInGame) 
            throw new PlayerAlreadyInSessionException("Player is already in an active lobby or game.");
    }
    
    protected virtual string GenerateCode()
    {
        return Guid.NewGuid().ToString("N")[..6].ToUpper();
    }

    public BattleshipEngine? JoinLobby(string gameCode, Player player2)
    {
        ArgumentNullException.ThrowIfNull(player2);
        CheckLobbyAndGame(player2);
        if (string.IsNullOrWhiteSpace(gameCode)) return null;

        if (!_lobbies.TryRemove(gameCode, out var player1)) return null;
        var engine = new BattleshipEngine(new GameBoard(), new GameBoard(), player1, player2);
        _games.TryAdd(gameCode, engine); 
        
        return engine;
    }
    
    public BattleshipEngine? GetGame(string? gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode)) return null;
        _games.TryGetValue(gameCode, out var engine);
        return engine;
    }

    public bool AddConnection(AddConnectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ConnectionId) || request.PlayerId == Guid.Empty) 
            throw new ArgumentException("ConnectionId and/or Guid cannot be null or empty.");
        
        return _connections.TryAdd(request.ConnectionId, request.PlayerId);
    }
    
    public async Task<string?> HandleDisconnectAsync(string connectionId, TimeSpan delay = default)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            throw new ArgumentException("Connection ID cannot be null or whitespace");

        if (!_connections.TryRemove(connectionId, out var playerId))
            return null;

        await Task.Delay(delay);

        if (_connections.Values.Contains(playerId))
            return null;

        var lobbyKey = _lobbies
            .FirstOrDefault(lobby => lobby.Value.Id == playerId)
            .Key;

        if (lobbyKey is not null)
        {
            _lobbies.TryRemove(lobbyKey, out _);
            return null;
        }

        var gameKey = _games
            .FirstOrDefault(game => game.Value.Players
                .Any(player => player.Id == playerId))
            .Key;

        if (gameKey is null)
            return null;

        _games.TryRemove(gameKey, out _);
        return gameKey;
    }
}