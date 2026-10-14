using Ardalis.Specification;
using ChatService.Entities;
using ChatService.Interfaces;
using ChatService.Specifications;
using ChatService.Data;
using Common.Messaging.Outbox;
using TMApi.Contracts;

namespace ChatService.Services;
public class MessageService : IMessageService
{
    private readonly IRepositoryBase<ChatMessage> _repository;
    private readonly ApplicationDbContext _db;

    public MessageService(IRepositoryBase<ChatMessage> repository, ApplicationDbContext db)
    {
        _repository = repository;
        _db = db;
    }
    public async Task<ChatMessage> SaveMessageAsync(ChatMessage message)
    {
        if (!message.ReceiverId.HasValue)
        {
            return await _repository.AddAsync(message);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var savedMessage = await _repository.AddAsync(message);

        var created = new ChatMessageCreatedV1
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = savedMessage.SentAt,
            MessageId = savedMessage.Id,
            SenderId = savedMessage.SenderId,
            ReceiverId = savedMessage.ReceiverId!.Value
        };

        var outbox = OutboxMessage.Create(created.EventId, created.OccurredAtUtc, ChatMessageCreatedV1.RoutingKey, created);

        _db.Set<OutboxMessage>().Add(outbox);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return savedMessage;
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesByRoomAsync(string room, int take = 50)
    {
        return await _repository.ListAsync(new MessageGetByRoomSpecification(room, take));
    }

    public async Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(int userId, int otherUserId, int take = 50)
    {
        return await _repository.ListAsync(new MessageGetConversationSpecification(userId, otherUserId, take));
    }
}
