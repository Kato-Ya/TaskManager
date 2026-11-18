using ChatService.Dto;

namespace ChatService.Interfaces;
public interface IChatRoomService
{
    Task<IEnumerable<ChatRoomDto>> GetUserRoomsAsync(int userId);
    Task<ChatRoomDto> CreateRoomAsync(CreateChatRoomDto createChatRoomDto, int userId);
}
