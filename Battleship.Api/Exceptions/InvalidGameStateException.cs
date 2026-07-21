namespace Battleship.Api.Exceptions;

public class InvalidGameStateException : Exception
{
    public InvalidGameStateException(){}
    
    public InvalidGameStateException(string message) : base(message){}
}