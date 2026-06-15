using Battleship.Api.Parsers;
using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;
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
            var expectedCoordinate = new Coordinate(expectedX, expectedY);

            var result = CoordinateParser.StringToCoord(input);

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
            var expectedCoordinate = new Coordinate(expectedX, expectedY);

            var result = CoordinateParser.StringToCoord(input);

            result.Should().BeEquivalentTo(expectedCoordinate);
        }

        [Theory]
        [InlineData("K10")]
        [InlineData("L5")]
        [InlineData("Z1")]
        [InlineData("A11")]
        [InlineData("E15")]
        [InlineData("J11")]
        [InlineData("M20")]
        public void StringToCoord_ThrowsException_WithCoordinatesOutsideGrid(string input)
        {
            var action = () => CoordinateParser.StringToCoord(input);

            action.Should().Throw<InvalidCoordinateException>()
                .WithMessage($"Invalid coordinate: {input}");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("1A")]
        [InlineData("AA")]
        [InlineData("A")]
        [InlineData("10")]
        [InlineData("A-1")]
        [InlineData(null)]
        public void StringToCoord_ThrowsException_WithMalformedCoordinates(string input)
        {
            var action = () => CoordinateParser.StringToCoord(input);

            action.Should().Throw<InvalidCoordinateException>()
                .WithMessage($"Invalid coordinate: {input}");
        }
        
        [Theory]
        [InlineData(" A1")]
        [InlineData("A1 ")]
        [InlineData(" A1 ")]
        [InlineData("\tA1")]
        [InlineData("A1\t")]
        public void StringToCoord_ThrowsException_WithCoordinatesContainingWhitespace(string input)
        {
            var action = () => CoordinateParser.StringToCoord(input);

            action.Should().Throw<InvalidCoordinateException>()
                .WithMessage($"Invalid coordinate: {input}");
        }
    }
}
