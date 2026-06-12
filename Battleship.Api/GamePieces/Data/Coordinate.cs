namespace Battleship.Api.GamePieces.Data
{
    public class Coordinate (int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
