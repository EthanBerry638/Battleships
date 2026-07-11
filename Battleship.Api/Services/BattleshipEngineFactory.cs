using Battleship.Api.Engine;
using Battleship.Api.GamePieces.Board;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.Services;

public class BattleshipEngineFactory : IBattleshipEngineFactory
{
    private readonly IGameBoard _player1Board;
    private readonly IGameBoard _player2Board;
    private readonly Player _player1;
    private readonly Player _player2;

    public BattleshipEngineFactory(IGameBoard player1Board, IGameBoard player2Board, Player player1, Player player2)
    {
        ArgumentNullException.ThrowIfNull(player1Board);
        ArgumentNullException.ThrowIfNull(player2Board);
        ArgumentNullException.ThrowIfNull(player1);
        ArgumentNullException.ThrowIfNull(player2);

        _player1Board = player1Board;
        _player2Board = player2Board;
        _player1 = player1;
        _player2 = player2;
    }

    public BattleshipEngine Create()
    {
        return new BattleshipEngine(_player1Board, _player2Board, _player1, _player2);
    }
}