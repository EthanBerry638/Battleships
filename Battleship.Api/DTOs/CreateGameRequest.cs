namespace Battleship.Api.DTOs;

public record CreateLobbyRequest(
    Guid PlayerId,
    string PlayerName);