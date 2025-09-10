using ChatService.Entities;
using ChatService.Interfaces;
using ChatService.Dto;
using ChatService.Protos;
using ChatService.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs;
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly GrpcUserClientService _grpcUserClient;

    public ChatHub(IMessageService messageService, GrpcUserClientService grpcUserClient)
    {
        _messageService = messageService;
        _grpcUserClient = grpcUserClient;
    }

    public async Task SendMessage(string room, int senderId, string senderName, string text)
    {
        var user = await _grpcUserClient.GetUserByIdAsync(senderId);
        if (user == null)
        {
            throw new HubException($"$User with id {senderId} not found");
        } 

        var message = new ChatMessage
        {
            Room = room ?? "global",
            SenderId = senderId,
            SenderName = user.Username,
            Text = text,
            SentAt = DateTime.UtcNow
        };

        var savedMessage = await _messageService.SaveMessageAsync(message);

        await Clients.Group(message.Room).SendAsync("ReceiveMessage", new
        {
            id = savedMessage.Id,
            room = savedMessage.Room,
            senderId = savedMessage.SenderId,
            senderName = savedMessage.SenderName,
            text = savedMessage.Text,
            sentAt = savedMessage.SentAt
        });
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
