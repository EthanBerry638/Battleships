using Battleship.Api.GamePieces.Data;
using Battleship.Api.Exceptions;
using FluentAssertions;

namespace Battleship.Tests.Unit_Tests.Coordinate_Tests;

public class CoordinateTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 9)]
    [InlineData(9, 0)]
    [InlineData(9, 9)]
    [InlineData(4, 5)]
    public void Validate_WithValidCoordinates_ShouldPassValidation(int x, int y)
    {
        var act = () => new Coordinate(x, y);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(-1, 5, "X must be between 0 and 9. Got -1.")]
    [InlineData(10, 5, "X must be between 0 and 9. Got 10.")]
    [InlineData(5, -1, "Y must be between 0 and 9. Got -1.")]
    [InlineData(5, 10, "Y must be between 0 and 9. Got 10.")]
    [InlineData(-1, -1, "X must be between 0 and 9. Got -1.")]
    [InlineData(10, 10, "X must be between 0 and 9. Got 10.")]
    public void Validate_WithInvalidCoordinates_ShouldFailValidationWithExpectedErrorMessage(
        int x,
        int y,
        string expectedErrorMessage)
    {
        var act = () => new Coordinate(x, y);

        act.Should().Throw<InvalidCoordinateException>()
            .WithMessage(expectedErrorMessage);
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 9)]
    [InlineData(9, 0)]
    [InlineData(9, 9)]
    [InlineData(4, 5)]
    public void Properties_ShouldInitialiseCorrectly_WhenPassedCorrectArguments(int x, int y)
    {
        var coordinate = new Coordinate(x, y);
        
        coordinate.X.Should().Be(x);
        coordinate.Y.Should().Be(y);
    }
    
    [Theory]
    [InlineData(2, 3, 2, 3, true)]
    [InlineData(2, 3, 2, 4, false)]
    [InlineData(2, 3, 3, 3, false)]
    public void Equality_ComparesValueSemanticsCorrectly(
        int x1, int y1,
        int x2, int y2,
        bool shouldBeEqual)
    {
        var first = new Coordinate(x1, y1);
        var second = new Coordinate(x2, y2);

        if (shouldBeEqual)
        {
            first.Should().Be(second);
            (first == second).Should().BeTrue();
        }
        else
        {
            first.Should().NotBe(second);
            (first == second).Should().BeFalse();
        }
    }
}