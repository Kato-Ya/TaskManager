using ChatService.Entities;
using ChatService.Interfaces;
using ChatService.Dto;
using Microsoft.AspNetCore.SignalR;
using ChatService.ConnectionManager;
using Microsoft.AspNetCore.Authorization;
using Common.Auth;

namespace ChatService.Hubs;
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IConnectionManager _connectionManager;

    private readonly IRoomAccessService _roomAccessService;

    public ChatHub(
        IChatService chatService,
        IConnectionManager connectionManager,
            IRoomAccessService roomAccessService)
    {
        _chatService = chatService;
        _connectionManager = connectionManager;
        _roomAccessService = roomAccessService;
    }

    public async Task SendMessage(CreateChatMessageDto createChatMessageDto)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            throw new HubException("User is not authenticated");
        }

        createChatMessageDto.SenderId = userId.Value;
        await _chatService.SendMessageAsync(createChatMessageDto);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            _connectionManager.AddConnection(userId.Value, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionManager.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomName)
    {
        int userId = GetCurrentUserId() ?? throw new HubException("User is not authenticated");

        bool canAcess = await _roomAccessService.CanAccessRoomAsync(roomName, userId);

        if (!canAcess)
        {
            throw new HubException("Access to room denied");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }

    public Task LeaveRoom(string room)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
    }

    private int? GetCurrentUserId()
    {
        return Context.User?.GetUserId();
    }
}
