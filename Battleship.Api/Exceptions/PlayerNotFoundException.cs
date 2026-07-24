namespace Battleship.Api.Exceptions;

public class PlayerNotFoundException(string message) : Exception(message);