using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.GamePieces.Board
{
    public class GameBoard : IGameBoard
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

        public Tile GetTile(Coordinate coordinate)
        {
            if (coordinate.X < 0 || coordinate.X >= 10 || coordinate.Y < 0 || coordinate.Y >= 10)
            {
                throw new InvalidCoordinateException($"Invalid coordinate: {coordinate}");
            }
            
            return _board[coordinate.X, coordinate.Y];
        }

        public PlacementResult PlaceShip(Ship ship)
        {
            List<Coordinate> invalidCoordinates = [];
            
            foreach (var coordinate in ship.Coordinates)
            {
                if (GetTile(coordinate).OccupyingShip == null)
                {
                    continue;
                }
                
                invalidCoordinates.Add(coordinate);
            }
            
            if (invalidCoordinates.Count != 0)
            {
                return new PlacementResult(false, invalidCoordinates);
            }

            foreach (var coordinate in ship.Coordinates)
            {
                _board[coordinate.X, coordinate.Y].OccupyingShip = ship;
            }

            return new PlacementResult(true);
        }
    }
}