namespace Battleship.Api.Exceptions;

public class PlayerAlreadyInSessionException : Exception
{
    public PlayerAlreadyInSessionException(string message) : base(message){}
}