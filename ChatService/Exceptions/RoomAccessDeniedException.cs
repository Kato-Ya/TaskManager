namespace ChatService.Exceptions;

public class RoomAccessDeniedException : Exception
{
    public RoomAccessDeniedException(string message) : base(message)
    {
    }
}
