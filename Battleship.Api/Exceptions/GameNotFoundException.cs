namespace Battleship.Api.Exceptions;

public class GameNotFoundException(string message) : Exception(message);