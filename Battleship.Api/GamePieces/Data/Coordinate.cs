using Battleship.Api.Exceptions;

namespace Battleship.Api.GamePieces.Data;

public record Coordinate
{
    public int X { get; }
    public int Y { get; }

    public Coordinate(int x, int y)
    {
        if (x is < 0 or > 9)
            throw new InvalidCoordinateException($"X must be between 0 and 9. Got {x}.");
        if (y is < 0 or > 9)
            throw new InvalidCoordinateException($"Y must be between 0 and 9. Got {y}.");

        X = x;
        Y = y;
    }
}