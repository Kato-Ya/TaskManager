namespace Common.Messaging;

public class InvalidIntegrationEventException : Exception
{
    public InvalidIntegrationEventException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}