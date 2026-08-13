namespace Battleship.Api.Exceptions;

public class InvalidCoordinateException(string message) : BattleshipException(message);