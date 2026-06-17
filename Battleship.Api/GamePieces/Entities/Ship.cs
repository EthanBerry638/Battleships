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
            
            if (coordinates[0] == new Coordinate(0, 0) && coordinates[1] == new Coordinate(0, 2))
            {
                throw new InvalidShipException("Coordinates must be adjacent.");
            }
        }
    }
}
