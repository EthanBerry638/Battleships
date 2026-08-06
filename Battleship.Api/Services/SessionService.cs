using Battleship.Api.Repositories;

namespace Battleship.Api.Services;

public class SessionService (ILobbyRepository lobbyRepository, IGameRepository gameRepository)
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
}