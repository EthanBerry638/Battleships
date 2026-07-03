namespace Battleship.Api.Exceptions
{
    public class GameOverException : Exception
    {
        public GameOverException(){}
        public GameOverException(string message):base(message){}
    }
}