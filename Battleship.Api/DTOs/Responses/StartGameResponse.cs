using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs.Responses;

public record StartGameResponse(string GameCode, GameStartResult Result);