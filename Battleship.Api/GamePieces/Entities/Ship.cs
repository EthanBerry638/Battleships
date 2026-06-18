using Battleship.Api.Exceptions;
using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities
{
    public class Ship : IShip
    {
        public ShipType Type { get; set; }
        public int Size { get; set; }
        public List<Coordinate> Coordinates { get; }
        private readonly List<Coordinate> _hits = [];

        public Ship(List<Coordinate> coordinates)
        {
            ArgumentNullException.ThrowIfNull(coordinates);
            ValidateCoordinates(coordinates);
            
            Coordinates = coordinates;
        }

        public void RegisterHit(Coordinate coordinate)
        {
            if (Coordinates.Contains(coordinate) && !_hits.Contains(coordinate))
            {
                _hits.Add(coordinate);
            }
        }

        public bool IsSunk()
        {
            return _hits.Count == Coordinates.Count;
        }

        private void ValidateCoordinates(List<Coordinate> coordinates)
        {
            if (coordinates.Count <= 1) return;
            
            var first = coordinates[0];
            bool isHorizontal = coordinates.All(c => c.Y == first.Y);
            bool isVertical = coordinates.All(c => c.X == first.X);

            if (!isHorizontal && !isVertical)
            {
                throw new InvalidShipException("Coordinates must be in a straight line.");
            }

            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                var currentCoordinate = coordinates[i];
                var nextCoordinate = coordinates[i + 1];
                
                var result = Math.Abs(currentCoordinate.X - nextCoordinate.X);
                var result2 = Math.Abs((currentCoordinate.Y - nextCoordinate.Y));
                
                if (result != 1 && result2 != 1)
                {
                    throw new InvalidShipException("Coordinates must be adjacent.");
                }
            }
        }
    }
}
