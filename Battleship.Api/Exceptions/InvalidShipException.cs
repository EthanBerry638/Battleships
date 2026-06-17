namespace Battleship.Api.Exceptions;

public class InvalidShipException : Exception
{
    public InvalidShipException(){}
    public InvalidShipException(string message):base(message){}
}