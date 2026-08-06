using Battleship.Api.Repositories;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Exceptions;
using Battleship.Api.Engine;

namespace Battleship.Api.Services;

public class SessionService (ILobbyRepository lobbyRepository, IGameRepository gameRepository)
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    
    public string CreateLobby(Player player1)
    {
        ArgumentNullException.ThrowIfNull(player1);
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
}