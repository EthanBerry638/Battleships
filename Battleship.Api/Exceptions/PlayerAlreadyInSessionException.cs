namespace Battleship.Api.Exceptions;

public class PlayerAlreadyInSessionException(string message) : BattleshipException(message);