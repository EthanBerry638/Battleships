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
    }
}
