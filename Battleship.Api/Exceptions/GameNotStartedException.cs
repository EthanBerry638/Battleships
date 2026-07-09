namespace Battleship.Api.Exceptions;

public class GameNotStartedException : Exception
{
    public GameNotStartedException(){}
    public GameNotStartedException(string message):base(message){}
}