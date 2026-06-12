using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;    
using System.Text.RegularExpressions;

namespace Battleship.Api.Parsers
{
    public class CoordinateParser
    {
        private const char FirstColumn = 'A';
        private const char LastColumn = 'J';
        private const int MaxRow = 10;
        private static readonly Regex CoordinateRegex = new(@"^[A-J](10|[1-9])$", RegexOptions.IgnoreCase);

        public static Coordinate StringToCoord(string input)
        {
            if (string.IsNullOrEmpty(input) || !CoordinateRegex.IsMatch(input))
            {
                throw new InvalidCoordinateException($"Invalid coordinate: {input}");   
            }
            
            input = input.ToUpper();
            
            int x = input[0] - FirstColumn;
            int y = int.Parse(input[1..]) - 1;

            return new Coordinate(x, y);
        }
    }
}
