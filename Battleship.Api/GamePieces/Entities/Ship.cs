using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.GamePieces.Entities
{
    public class Ship
    {
        public ShipType Type { get; set; }
        public int Size { get; set; }

        public bool IsSunk ()
        {
            return true;
        }
    }
}
