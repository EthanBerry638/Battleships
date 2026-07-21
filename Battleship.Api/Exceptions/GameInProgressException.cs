namespace Battleship.Api.Exceptions;

public class GameInProgressException : Exception
{
    public GameInProgressException(){}
    
    public GameInProgressException(string message) : base(message){}
}