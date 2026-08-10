namespace Battleship.Api.DTOs;

public record StartGameRequest(
    Guid Player1Id,
    Guid Player2Id);