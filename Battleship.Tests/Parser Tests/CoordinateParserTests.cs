using Xunit;
using Battleship.Api.Parsers;
using Battleship.Api.GamePieces.Data;
using FluentAssertions;

namespace Battleship.Tests.Parser_Tests
{
    public class CoordinateParserTests
    {
        [Fact]
        public void StringToCoord_ReturnsCorrectCoordinates_WhenGivenValidInput()
        {
            var parser = new CoordinateParser();
            var input = "A1";
            var expectedCoordinates = new Coordinate(0, 0);

            var result = parser.StringToCoord(input);

            result.Should().BeEquivalentTo(expectedCoordinates);
        }
    }
}
