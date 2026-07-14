using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Services;

public interface IBattleshipManager
{
    string CreateLobby(Player player);
    BattleshipEngine? GetGame (string gameCode);
}