using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;    

namespace Battleship.Api.Parsers
{
    public class CoordinateParser
    {
        private const char FirstColumn = 'A';
        private const char LastColumn = 'J';
        private const int MaxRow = 10;

        public Coordinate StringToCoord(string input)
        {
            input = input.ToUpper();

            if (input[0] > LastColumn || int.Parse(input[1..]) > MaxRow)
            {
                throw new InvalidCoordinateException($"Invalid coordinate: {input}");
            }

            int x = input[0] - FirstColumn;
            int y = int.Parse(input[1..]) - 1;

            return new Coordinate(x, y);
        }
    }
}
