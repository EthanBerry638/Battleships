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


// TODO: Add parser class tomorrow to convert from string to coordinate and vice versa. This will be used for the API endpoints.
// For example, "A1" will be converted to Coordinate(0, 0) and "B2" will be converted to Coordinate(1, 1). This will make it easier for the client to interact with the API.