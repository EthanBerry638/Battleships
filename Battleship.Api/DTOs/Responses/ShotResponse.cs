using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs.Responses;

public record ShotResponse(
    ShotResult Result,
    string GameCode,
    Guid ShooterId,
    Coordinate Coordinate);