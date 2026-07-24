using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.DTOs;

public record PlaceShipRequest(
    Guid PlayerId,
    IShip Ship);