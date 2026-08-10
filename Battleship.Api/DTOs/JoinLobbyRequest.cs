namespace Battleship.Api.DTOs;

public record JoinLobbyRequest(
    string GameCode,
    Guid PlayerId,
    string PlayerName);