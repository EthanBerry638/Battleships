namespace Battleship.Api.DTOs;

public record DisconnectResponse(
    bool IsSuccessful,
    string? GameCode);