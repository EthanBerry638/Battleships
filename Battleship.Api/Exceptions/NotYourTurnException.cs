namespace Battleship.Api.Exceptions;

public class NotYourTurnException : Exception
{
    public NotYourTurnException(){}
    public NotYourTurnException(string message):base(message){}   
}