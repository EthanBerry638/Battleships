using Battleship.Api.Repositories;

namespace Battleship.Api.Services;

public class GameService (IGameRepository gameRepository)
{
    private readonly IGameRepository _gameRepository = gameRepository;
}