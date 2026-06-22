namespace Battleship.Api.GamePieces.Data;

public record PlacementResult(
    bool IsSuccessful, 
    List<Coordinate>? InvalidCoordinates = null);