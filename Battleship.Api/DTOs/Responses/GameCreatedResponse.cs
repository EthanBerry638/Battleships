using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.DTOs.Responses;

public record GameCreatedResponse(
    Player StartingPlayer,
    IReadOnlyList<PlayerSummary> Players);