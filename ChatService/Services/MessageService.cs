using Ardalis.Specification;
using ChatService.Entities;
using ChatService.Interfaces;
using ChatService.Specifications;

namespace ChatService.Services;
public class MessageService : IMessageService
{
    private readonly IRepositoryBase<ChatMessage> _repository;

    public MessageService(IRepositoryBase<ChatMessage> repository)
    {
        _repository = repository;
    }
    public async Task<ChatMessage> SaveMessageAsync(ChatMessage message)
    {
        return await _repository.AddAsync(message);
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesByRoomAsync(string room, int take = 50)
    {
        return await _repository.ListAsync(new MessageGetByRoomSpecification(room, take));
    }

}
