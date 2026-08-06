using Battleship.Api.Repositories;

namespace Battleship.Api.Services;

public class LobbyService (ILobbyRepository lobbyRepository)
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
}