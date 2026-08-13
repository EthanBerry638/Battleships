namespace Battleship.Api.DTOs.Requests;

public record CreateLobbyRequest(
    Guid PlayerId,
    string PlayerName);