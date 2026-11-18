using Ardalis.Specification;
using ChatService.Dto;
using ChatService.Entities;
using ChatService.Exceptions;
using ChatService.Interfaces;
using ChatService.Specifications;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ChatService.Services;
public class ChatRoomService : IChatRoomService
{
    private readonly IRepositoryBase<ChatRoom> _repository;
    private readonly IRepositoryBase<ChatMessage> _messageRepository;

    public ChatRoomService(IRepositoryBase<ChatRoom> repository, IRepositoryBase<ChatMessage> messageRepository)
    {
        _repository = repository;
        _messageRepository = messageRepository;
    }

    public async Task<IEnumerable<ChatRoomDto>> GetUserRoomsAsync(int userId)
    {
        return await _repository.ListAsync(new ChatRoomByUserSpecification(userId));
    }

    public async Task<ChatRoomDto> CreateRoomAsync(CreateChatRoomDto createChatRoomDto, int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("Invalid user id.");
        }

        var roomName = createChatRoomDto.RoomName?.Trim();
        if (string.IsNullOrWhiteSpace(roomName) || roomName.Length > 50)
        {
            throw new ArgumentException("Room name must contain between 1 and 50 characters.");
        }

        if (roomName.Contains('/') || roomName.Contains('\\') 
                                   || roomName == "." || roomName == ".."
                                   || roomName.Any(char.IsControl))
        {
            throw new ArgumentException("Room name contains invalid characters.");
        }

        if (string.Equals(roomName, "global", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The room name 'global' is reserved.");
        }

        var hasHistory = await _messageRepository.AnyAsync(new GroupMessageByRoomSpecification(roomName));
        if (hasHistory)
        {
            throw new RoomAlreadyExistsException("Room with this name already exists.");
        }

        var room = new ChatRoom
        {
            RoomName = roomName,
            OwnerId = userId,
            ChatRoomMembers = new List<ChatRoomMember>
            {
                new ChatRoomMember
                {
                    UserId = userId
                }
            }
        };

        try
        {
            await _repository.AddAsync(room);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException postgresException
                  && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
                  && postgresException.ConstraintName == "IX_chat_room_room_name")
        {
            throw new RoomAlreadyExistsException("Room with this name already exists.");
        }

        return new ChatRoomDto
        {
            Id = room.Id,
            RoomName = room.RoomName,
            OwnerId = room.OwnerId
        };
    }
}
