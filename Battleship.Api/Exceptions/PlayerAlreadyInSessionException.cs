namespace Battleship.Api.Exceptions;

public class PlayerAlreadyInSessionException(string message) : Exception(message);