using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities
{
    public class Ship (List<Coordinate> coordinates)
    {
        public ShipType Type { get; set; }
        public int Size { get; set; }
        public List<Coordinate> Coordinates = coordinates;

        public bool IsSunk ()
        {
            return true;
        }
    }
}
