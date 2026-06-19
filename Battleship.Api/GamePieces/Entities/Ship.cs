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
        
        //TODO: Add validation for ship size against the ship type
        
        private void ValidateCoordinates(List<Coordinate> coordinates)
        {
            if (coordinates.Count <= 1) throw new InvalidShipException("A ship must occupy at least 2 coordinates.");
            
            var firstCoord = coordinates[0];
            bool isHorizontal = coordinates.All(c => c.Y == firstCoord.Y);
            bool isVertical = coordinates.All(c => c.X == firstCoord.X);
            bool isDuplicated = coordinates.Distinct().Count() != coordinates.Count;

            if (isDuplicated) throw new InvalidShipException("Coordinates must be unique.");
            
            if (!isHorizontal && !isVertical)
            {
                throw new InvalidShipException("Coordinates must be in a straight line.");
            }

            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                var currentCoordinate = coordinates[i];
                var nextCoordinate = coordinates[i + 1];
                
                var xDifference = Math.Abs(currentCoordinate.X - nextCoordinate.X);
                var yDifference = Math.Abs((currentCoordinate.Y - nextCoordinate.Y));
                
                if (xDifference != 1 && yDifference != 1)
                {
                    throw new InvalidShipException("Coordinates must be adjacent.");
                }
            }
        }
    }
}
