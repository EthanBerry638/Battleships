using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs.Responses;

public record ShotMessage(
    ShotResult Result,
    Guid ShooterId,
    Coordinate Coordinate);