namespace TMApi.Contracts;

public class ChatMessageCreatedV1
{
    public const string RoutingKey = "chat.message-created.v1";

    public Guid EventId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int MessageId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
}
