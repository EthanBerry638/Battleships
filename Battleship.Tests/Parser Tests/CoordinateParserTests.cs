using Battleship.Api.Parsers;
using Battleship.Api.GamePieces.Data;
using FluentAssertions;

namespace Battleship.Tests.Parser_Tests
{
    public class CoordinateParserTests
    {
        [Theory]
        [InlineData("A1", 0, 0)]
        [InlineData("A2", 0, 1)]
        [InlineData("B1", 1, 0)]
        [InlineData("B2", 1, 1)]
        [InlineData("J10", 9, 9)]
        public void StringToCoord_ReturnsCorrectCoordinates_WithValidInputs(string input, int expectedX, int expectedY)
        {
            var parser = new CoordinateParser();
            var expectedCoordinate = new Coordinate(expectedX, expectedY);

            var result = parser.StringToCoord(input);

            result.Should().BeEquivalentTo(expectedCoordinate);
        }

        [Theory]
        [InlineData("a1", 0, 0)]
        [InlineData("a2", 0, 1)]
        [InlineData("b1", 1, 0)]
        [InlineData("b2", 1, 1)]
        [InlineData("j10", 9, 9)]
        public void StringToCoord_ReturnsCorrectCoordinates_WithLowercaseLetters(string input, int expectedX, int expectedY)
        {
            var parser = new CoordinateParser();
            var expectedCoordinate = new Coordinate(expectedX, expectedY);

            var result = parser.StringToCoord(input);

            result.Should().BeEquivalentTo(expectedCoordinate);
        }
    }
}
