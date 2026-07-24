namespace Battleship.Api.DTOs;

public record JoinLobbyRequest(
    Guid PlayerId,
    string PlayerName);