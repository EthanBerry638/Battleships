using Battleship.Api.Repositories;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Exceptions;
using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Board; 

namespace Battleship.Api.Services;

public class SessionService (ILobbyRepository lobbyRepository, IGameRepository gameRepository) : ISessionService
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    
    public string CreateLobby(Player player1)
    {
        CheckLobbyAndGame(player1.Id);

        string gameCode;
        do
        {
            gameCode = Guid.NewGuid().ToString("N")[..6].ToUpper();
        } while (!_lobbyRepository.TryAddLobby(gameCode, player1));

        return gameCode;
    }

    private void CheckLobbyAndGame(Guid playerId)
    {
        bool isWaitingInLobby = _lobbyRepository.IsPlayerInLobby(playerId);
        bool isPlayingInGame = _gameRepository.IsPlayerInGame(playerId);

        if (isWaitingInLobby || isPlayingInGame)
            throw new PlayerAlreadyInSessionException("Player is already in an active lobby or game.");
    }

    public BattleshipEngine? GetGame(string gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode)) return null;
        _gameRepository.TryGetGameByCode(gameCode, out GameSession? session);
        return session?.Engine;
    }
    
    public BattleshipEngine? JoinLobby(string gameCode, Player player2)
    {
        ArgumentNullException.ThrowIfNull(player2);
        CheckLobbyAndGame(player2.Id);
        if (string.IsNullOrWhiteSpace(gameCode)) return null;

        if (!_lobbyRepository.TryRemoveLobby(gameCode, out Player? player1)) return null;
        var engine = new BattleshipEngine(new GameBoard(), new GameBoard(), player1!, player2);
        var session = new GameSession(engine);
        _gameRepository.TryAddGame(gameCode, session);

        return engine;
    }
}