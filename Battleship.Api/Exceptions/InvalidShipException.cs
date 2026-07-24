namespace Battleship.Api.Exceptions;

public class InvalidShipException(string message) : Exception(message);