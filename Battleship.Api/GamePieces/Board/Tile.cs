using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.GamePieces.Board
{
    public class Tile
    {
        public bool IsHit { get; set; }
        public Ship? OccupyingShip { get; set; }
        public bool HasShip => OccupyingShip != null;
    }
}
