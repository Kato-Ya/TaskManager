using ChatService.Entities;
using ChatService.Interfaces;
using ChatService.Dto;
using ChatService.GrpcServices;
using ChatService.Services;
using Microsoft.AspNetCore.SignalR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Collections.Concurrent;
using ChatService.ConnectionManager;

namespace ChatService.Hubs;
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly GrpcUserClientService _grpcUserClient;
    private readonly GrpcNotificationClientService _grpcNotificationClient;
    private readonly IChatService _chatService;
    private readonly IConnectionManager _connectionManager;

    public ChatHub(IMessageService messageService,
        GrpcUserClientService grpcUserClient,
        GrpcNotificationClientService grpcNotificationClient,
        IChatService chatService,
        IConnectionManager connectionManager)
    {
        _messageService = messageService;
        _grpcUserClient = grpcUserClient;
        _grpcNotificationClient = grpcNotificationClient;
        _chatService = chatService;
        _connectionManager = connectionManager;
    }

    public async Task SendMessage(CreateChatMessageDto createChatMessageDto)
    {
        await _chatService.SendMessageAsync(createChatMessageDto);
    }

    public override async Task OnConnectedAsync()
    {
        //await base.OnConnectedAsync();
        var userId = int.Parse(Context.GetHttpContext()!.Request.Query["userId"]!);
        _connectionManager.AddConnection(userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionManager.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public Task JoinRoom(string room)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, room);
    }

    public Task LeaveRoom(string room)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
    }
}
