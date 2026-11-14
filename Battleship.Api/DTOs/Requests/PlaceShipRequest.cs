using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs.Requests;

public record PlaceShipRequest(
    ShipType Type,
    List<Coordinate> Coordinates);