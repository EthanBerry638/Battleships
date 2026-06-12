using System.Text.RegularExpressions;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;    

namespace Battleship.Api.Parsers
{
    public static class CoordinateParser
    {
        private const char ColumnOffset = 'A';

        private static readonly Regex CoordinateRegex = new(@"^[A-J](10|[1-9])$", RegexOptions.IgnoreCase);

        public static Coordinate StringToCoord(string input)
        {
            if (string.IsNullOrEmpty(input) || !CoordinateRegex.IsMatch(input))
            {
                throw new InvalidCoordinateException($"Invalid coordinate: {input}");
            }
        
            var normalizedInput = input.ToUpperInvariant();

            int x = normalizedInput[0] - ColumnOffset;
            int y = int.Parse(normalizedInput[1..]) - 1;

            return new Coordinate(x, y);
        }
    }
}
