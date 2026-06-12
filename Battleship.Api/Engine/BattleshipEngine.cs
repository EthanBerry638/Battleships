using Battleship.Api.GamePieces.Board;

namespace Battleship.Api.Engine
{
    public class BattleshipEngine (IGameBoard gameBoard)
    {
        private readonly IGameBoard _gameBoard = gameBoard;
    }
}
