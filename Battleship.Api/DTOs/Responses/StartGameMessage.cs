namespace Battleship.Api.DTOs.Responses;

public record StartGameMessage(
    bool IsStarted,
    Guid? StartingPlayerId,
    IReadOnlyList<Guid>? PlayerIds);