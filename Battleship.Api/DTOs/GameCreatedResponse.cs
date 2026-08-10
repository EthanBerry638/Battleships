using Battleship.Api.GamePieces.Entities;

namespace Battleship.Api.DTOs;

public record GameCreatedResponse(
    Player StartingPlayer,
    Guid Player1Id,
    Guid Player2Id);