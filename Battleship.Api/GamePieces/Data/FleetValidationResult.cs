namespace Battleship.Api.GamePieces.Data;

public record FleetValidationResult(
    bool IsValid,
    List<ShipType> MissingShips,
    List<ShipType> ExtraShips
    );