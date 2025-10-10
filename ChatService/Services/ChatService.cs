using System.Collections.Concurrent;
using ChatService.Dto;
using ChatService.Entities;
using ChatService.Hubs;
using ChatService.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using ChatService.GrpcServices;
using ChatService.ConnectionManager;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatService.Services;
public class ChatService : IChatService
{
    private readonly IMessageService _messageService;
    private readonly GrpcUserClientService _grpcUserClient;
    private readonly GrpcNotificationClientService _grpcNotificationClient;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IConnectionManager _connectionManager;


    public ChatService(
        IMessageService messageService,
        GrpcUserClientService grpcUserClient,
        GrpcNotificationClientService grpcNotificationClient,
        IHubContext<ChatHub> hubContext, 
        IConnectionManager connectionManager)
    {
        _messageService = messageService;
        _grpcUserClient = grpcUserClient;
        _grpcNotificationClient = grpcNotificationClient;
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

        //await _hubContext.Clients.Group(chatMessageDtoResult.Room!).SendAsync("ReceiveMessage", chatMessageDtoResult);

        //if (savedMessage.ReceiverId.HasValue)
        //{

        //    await _grpcNotificationClient.SendMessageNotificationAsync(
        //        savedMessage.ReceiverId.Value,  
        //        $"Вам пришло сообщение: {savedMessage.Text}",
        //        savedMessage.Id
        //    );
        //}

        if (savedMessage.ReceiverId.HasValue)
        {
            // personal messages
            if (_connectionManager.TryGetConnection(savedMessage.ReceiverId.Value, out var connectionIds))
            {
                foreach (var connectionId in connectionIds)
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveMessage", chatMessageDtoResult);

                }
            }

            await _grpcNotificationClient.SendMessageNotificationAsync(
                savedMessage.ReceiverId.Value,
                $"Вам пришло сообщение: {savedMessage.Text}",
                savedMessage.Id
            );
        }
        else
        {
            // group's messages
            await _hubContext.Clients.Group(chatMessageDtoResult.Room!)
                .SendAsync("ReceiveMessage", chatMessageDtoResult);
        }


        return chatMessageDtoResult;
    }
}
