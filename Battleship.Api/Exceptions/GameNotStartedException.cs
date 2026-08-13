namespace Battleship.Api.Exceptions;

public class GameNotStartedException(string message) : BattleshipException(message);