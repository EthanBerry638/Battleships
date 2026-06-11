using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Board
{
    public class GameBoard
    {
        private readonly Tile[,] _board = new Tile[10, 10];

        public GameBoard()
        {   
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    _board[x, y] = new Tile();
                }
            }
        }  
    }
}