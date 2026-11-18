namespace ChatService.Exceptions;

public class RoomAlreadyExistsException : Exception
{
    public RoomAlreadyExistsException(string message) : base(message)
    {
    }
}
