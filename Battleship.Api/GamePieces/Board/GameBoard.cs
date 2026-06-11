using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Board
{
    public class GameBoard
    {
        private readonly Tile[,] _board;
        private readonly BoardSize _boardSize;

        public GameBoard(BoardSize boardSize = null!)
        {
            _boardSize = boardSize ?? new BoardSize(10, 10);
            _board = new Tile[_boardSize.X, _boardSize.Y];
            
            for (int x = 0; x < _boardSize.X; x++)
            {
                for (int y = 0; y < _boardSize.Y; y++)
                {
                    _board[x, y] = new Tile();
                }
            }
        }  
    }
}