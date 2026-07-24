namespace Battleship.Api.Exceptions;

public class GameInProgressException(string message) : Exception(message);