using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;    

namespace Battleship.Api.Parsers
{
    public class CoordinateParser
    {
        public Coordinate StringToCoord(string input)
        {
            input = input.ToUpper();

            if (input[0] > 'J' || int.Parse(input[1..]) > 10)
            {
                throw new InvalidCoordinateException($"Invalid coordinate: {input}");
            }

            int x = (int)input[0] - (int)'A';
            int y = int.Parse(input[1..]) - 1;

            return new Coordinate(x, y);
        }
    }
}
