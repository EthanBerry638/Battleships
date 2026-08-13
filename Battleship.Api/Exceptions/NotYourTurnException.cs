namespace Battleship.Api.Exceptions;

public class NotYourTurnException(string message) : BattleshipException(message);