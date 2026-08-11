using System.ComponentModel.DataAnnotations;

namespace Battleship.Api.GamePieces.Data;

public record Coordinate(
    [property: Range(0, 9, ErrorMessage = "X must be between 0 and 9.")]
    int X,
    [property: Range(0, 9, ErrorMessage = "Y must be between 0 and 9.")]
    int Y
);