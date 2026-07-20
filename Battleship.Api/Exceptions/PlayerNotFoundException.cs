namespace Battleship.Api.Exceptions;

public class PlayerNotFoundException : Exception
{
    public PlayerNotFoundException(){}

    public PlayerNotFoundException(string message) : base(message){}
}