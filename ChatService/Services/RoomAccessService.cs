using Ardalis.Specification;
using ChatService.Entities;
using ChatService.Dto;
using ChatService.Interfaces;
using ChatService.Specifications;
using Common.Auth;

namespace ChatService.Services;
public class RoomAccessService : IRoomAccessService
{
    private readonly IRepositoryBase<ChatRoomMember> _repositoryChatRoomMember;
    private readonly IRepositoryBase<ChatRoom> _repositoryChatRoom;

    public RoomAccessService(IRepositoryBase<ChatRoomMember> repositoryChatRoomMember, IRepositoryBase<ChatRoom> repositoryChatRoom)
    {
        _repositoryChatRoomMember = repositoryChatRoomMember;
        _repositoryChatRoom = repositoryChatRoom;
    }

    public async Task<bool> IsMemberAsync(int roomId, int userId)
    {
        return await _repositoryChatRoomMember.AnyAsync(new RoomMembershipSpecification(roomId, userId));
    }

    public async Task<bool> CanAccessRoomAsync(string roomName, int userId)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            return false;
        }

        if (roomName == "global")
        {
            return true;
        }
        return await _repositoryChatRoom.AnyAsync(new RoomAccessSpecification(roomName, userId));
    }
}