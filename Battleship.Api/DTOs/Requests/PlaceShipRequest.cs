using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs.Requests;

public record PlaceShipRequest(
    Guid PlayerId,
    ShipType Type,
    List<Coordinate> Coordinates);