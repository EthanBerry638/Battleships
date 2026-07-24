using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Entities;
using Battleship.Api.DTOs;

namespace Battleship.Api.Services;

public interface IBattleshipManager
{
    string CreateLobby(Player player);
    BattleshipEngine? JoinLobby(string gameCode, Player player2);
    bool AddConnection(AddConnectionRequest request);
    Task<string?> HandleDisconnectAsync(string connectionId, TimeSpan delay = default);
}