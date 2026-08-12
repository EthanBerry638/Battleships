namespace Battleship.Api.Exceptions;

public class GameNotFoundException(string message) : BattleshipException(message);