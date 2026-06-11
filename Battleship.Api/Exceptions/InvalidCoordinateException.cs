namespace Battleship.Api.Exceptions
{
    public class InvalidCoordinateException : Exception
    {
        public InvalidCoordinateException(){}
        public InvalidCoordinateException(string message):base(message){}
    }
}
