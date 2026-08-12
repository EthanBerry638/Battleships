using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs;

public record StartGameResponse(string GameCode, GameStartResult Result);