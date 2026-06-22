namespace Battleship.Api.GamePieces.Data;

public record PlacementResult(bool IsSuccessful, PlacementResultReason Reason);

public enum PlacementResultReason
{
    Success,
    Collision,
    OutOfBounds
}