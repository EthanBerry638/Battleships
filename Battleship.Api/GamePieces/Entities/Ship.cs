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

            Coordinates = coordinates.OrderBy(c => c.X).ThenBy(c => c.Y).ToList();
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
            if (coordinates.Count <= 1) throw new InvalidShipException("A ship must occupy at least 2 coordinates.");

            var sortedCoordinates = coordinates.OrderBy(c => c.X).ThenBy(c => c.Y).ToList();

            var firstCoord = sortedCoordinates[0];
            bool isHorizontal = sortedCoordinates.All(c => c.Y == firstCoord.Y);
            bool isVertical = sortedCoordinates.All(c => c.X == firstCoord.X);
            bool isDuplicated = sortedCoordinates.Distinct().Count() != sortedCoordinates.Count;

            if (isDuplicated) throw new InvalidShipException("Coordinates must be unique.");

            if (!isHorizontal && !isVertical)
            {
                throw new InvalidShipException("Coordinates must be in a straight line.");
            }

            for (int i = 0; i < sortedCoordinates.Count - 1; i++)
            {
                var currentCoordinate = sortedCoordinates[i];
                var nextCoordinate = sortedCoordinates[i + 1];

                var xDifference = Math.Abs(currentCoordinate.X - nextCoordinate.X);
                var yDifference = Math.Abs((currentCoordinate.Y - nextCoordinate.Y));

                if (xDifference != 1 && yDifference != 1)
                {
                    throw new InvalidShipException("Coordinates must be adjacent.");
                }
            }
        }

        private bool IsValidShip(ShipType shipType, int size)
        {
            return true;
        }
    }
}
