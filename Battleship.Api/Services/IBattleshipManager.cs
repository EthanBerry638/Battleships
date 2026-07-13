using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Services;

public interface IBattleshipManager
{
    string CreateGame (Player player1, Player player2);
    BattleshipEngine? GetGame (string gameCode);
}