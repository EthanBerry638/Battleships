using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities
{
    public class Ship(List<Coordinate> coordinates)
    {
        public ShipType Type { get; set; }
        public int Size { get; set; }
        public List<Coordinate> Coordinates = coordinates;
        private readonly List<Coordinate> _hits = [];

        public void RegisterHit(Coordinate coordinate)
        {
            if (Coordinates.Contains(coordinate))
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
