using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs.Responses;

public record StartGameResponse(
    bool IsStarted,
    string? GameCode = null,
    Guid? StartingPlayerId = null,
    IReadOnlyList<Guid>? PlayerIds = null);