using ChatService.Entities;
using ChatService.Interfaces;
using ChatService.Dto;
using ChatService.Protos;
using ChatService.Services;
using Microsoft.AspNetCore.SignalR;
using TaskService.GrpcServices;

namespace ChatService.Hubs;
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly GrpcUserClientService _grpcUserClient;
    private readonly GrpcNotificationClientService _grpcNotificationClient;
    private readonly IChatService _chatService;

    public ChatHub(IMessageService messageService, GrpcUserClientService grpcUserClient, GrpcNotificationClientService grpcNotificationClient, IChatService chatService)
    {
        _messageService = messageService;
        _grpcUserClient = grpcUserClient;
        _grpcNotificationClient = grpcNotificationClient;
        _chatService = chatService;
    }

    public async Task SendMessage(CreateChatMessageDto createChatMessageDto)
    {
        await _chatService.SendMessageAsync(createChatMessageDto);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
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
