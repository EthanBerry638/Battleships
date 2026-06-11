using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.Parsers
{
    public class CoordinateParser
    {
        public Coordinate StringToCoord(string input)
        {
            int x = (int)input[0] - (int)'A';
            int y = int.Parse(input[1..]) - 1;

            return new Coordinate(x, y);
        }
    }
}
