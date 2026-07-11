namespace Battleship.Api.DTOs;

public record StartMatchDto(
    Guid Player1Id,
    Guid Player2Id);