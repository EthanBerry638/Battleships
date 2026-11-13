namespace Battleship.Api.GamePieces.Data;

public record ShotOutcome(
    ShotResult Result,
    IReadOnlyList<Coordinate>? SunkCoordinates);