namespace Battleship.Api.GamePieces.Data;

public record PlacementResult(
    bool IsSuccessful, 
    PlacementResultReason Reason,
    List<Coordinate>? Coordinates = null);

public enum PlacementResultReason
{
    Success,
    Collision,
    OutOfBounds
}