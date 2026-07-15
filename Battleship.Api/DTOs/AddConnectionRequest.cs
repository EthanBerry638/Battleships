namespace Battleship.Api.DTOs;

public record AddConnectionRequest(
    string ConnectionId,
    Guid PlayerId);