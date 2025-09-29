using ChatService.Dto;

namespace ChatService.Interfaces;
public interface IChatService
{
    Task<ChatMessageDto> SendMessageAsync(CreateChatMessageDto dto);
}
