namespace Battleship.Api.GamePieces.Data;

public record ShootResult(
    ShotResult Result,
    IReadOnlyList<Coordinate>? SunkCoordinates);