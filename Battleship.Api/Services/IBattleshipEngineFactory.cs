using Battleship.Api.Engine;

namespace Battleship.Api.Services;

public interface IBattleshipEngineFactory
{
    BattleshipEngine Create();
}