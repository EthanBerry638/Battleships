using Battleship.Api.Engine;

namespace Battleship.Api.Services;

public interface IBattleshipManager
{
    BattleshipEngine? GetGame (string gameCode);
}