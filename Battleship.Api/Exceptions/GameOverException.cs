namespace Battleship.Api.Exceptions;

public class GameOverException(string message) : BattleshipException(message);