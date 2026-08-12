using Battleship.Api.GamePieces.Data;
using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.DTOs;

public record PlaceShipRequest(
    Guid PlayerId,
    ShipType Type,
    List<Coordinate> Coordinates);