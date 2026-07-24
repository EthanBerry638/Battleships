namespace Battleship.Api.Exceptions;

public class GameOverException(string message) : Exception(message);