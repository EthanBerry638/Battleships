using Battleship.Api.Repositories;

namespace Battleship.Api.Services;

public class SessionService (ILobbyRepository lobbyRepository, IGameRepository gameRepository)
{
    private readonly ILobbyRepository _lobbyRepository = lobbyRepository;
    private readonly IGameRepository _gameRepository = gameRepository;
    
    protected virtual string GenerateCode()
    {
        return Guid.NewGuid().ToString("N")[..6].ToUpper();
    }
}