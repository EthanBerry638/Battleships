using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs.Requests;

public record ShootRequest(
    Guid PlayerId,
    Coordinate Coordinate);