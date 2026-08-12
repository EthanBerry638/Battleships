using Battleship.Api.GamePieces.Data;

namespace Battleship.Api.DTOs;

public record StartGameOutcome(string GameCode, GameStartResult Result);