using System.Collections.Concurrent;
using System.Security.Claims;
using ChatService.Dto;
using ChatService.Entities;
using ChatService.Hubs;
using ChatService.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using ChatService.ConnectionManager;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatService.Services;
public class ChatService : IChatService
{
    private readonly IMessageService _messageService;
    private readonly GrpcUserClientService _grpcUserClient;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IConnectionManager _connectionManager;


    public ChatService(
        IMessageService messageService,
        GrpcUserClientService grpcUserClient,
        IHubContext<ChatHub> hubContext, 
        IConnectionManager connectionManager)
    {
        _messageService = messageService;
        _grpcUserClient = grpcUserClient;
        _hubContext = hubContext;
        _connectionManager = connectionManager;
    }

    public async Task<ChatMessageDto> SendMessageAsync(CreateChatMessageDto createChatMessageDto)
    {
        var user = await _grpcUserClient.GetUserByIdAsync(createChatMessageDto.SenderId);
        if (user == null)
        {
            throw new HubException($"User with id {createChatMessageDto.SenderId} not found");
        }

        var message = new ChatMessage
        {
            Room = createChatMessageDto.Room ?? "global",
            SenderId = createChatMessageDto.SenderId,
            SenderName = user.Username,
            ReceiverId = createChatMessageDto.ReceiverId,
            Text = createChatMessageDto.Text,
            SentAt = DateTime.UtcNow
        };

        var savedMessage = await _messageService.SaveMessageAsync(message);

        var chatMessageDtoResult = new ChatMessageDto
        {
            Id = savedMessage.Id,
            Room = savedMessage.Room,
            SenderId = savedMessage.SenderId,
            SenderName = savedMessage.SenderName,
            ReceiverId = savedMessage.ReceiverId,
            Text = savedMessage.Text,
            SentAt = savedMessage.SentAt
        };

        if (savedMessage.ReceiverId.HasValue)
        {
            // personal messages
            await SendToUserConnectionsAsync(savedMessage.SenderId, chatMessageDtoResult);
            if (savedMessage.ReceiverId.Value != savedMessage.SenderId)
            {
                await SendToUserConnectionsAsync(savedMessage.ReceiverId.Value, chatMessageDtoResult);
            }

        }
        else
        {
            // group's messages
            await _hubContext.Clients.Group(chatMessageDtoResult.Room!)
                .SendAsync("ReceiveMessage", chatMessageDtoResult);
        }


        return chatMessageDtoResult;
    }

    private async Task SendToUserConnectionsAsync(int userId, ChatMessageDto message)
    {
        if (!_connectionManager.TryGetConnection(userId, out var connectionIds))
        {
            return;
        }

        foreach (var connectionId in connectionIds.Distinct())
        {
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveMessage", message);
        }
    }
}
