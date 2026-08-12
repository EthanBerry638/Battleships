namespace Battleship.Api.DTOs.Requests;

public record JoinLobbyRequest(
    string GameCode,
    Guid PlayerId,
    string PlayerName);