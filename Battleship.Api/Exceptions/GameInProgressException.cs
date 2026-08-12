namespace Battleship.Api.Exceptions;

public class GameInProgressException(string message) : BattleshipException(message);