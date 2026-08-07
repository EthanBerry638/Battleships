using Battleship.Api.GamePieces.Entities;
using Battleship.Api.Engine;

namespace Battleship.Api.Services;

public interface ISessionService
{
    string CreateLobby(Player player1);
    BattleshipEngine? GetGame(string gameCode);
    BattleshipEngine? JoinLobby(string gameCode, Player player2);
}