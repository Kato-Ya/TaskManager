namespace ChatService.Interfaces;
public interface IRoomAccessService
{
    Task<bool> IsMemberAsync(int roomId, int userId);
    Task<bool> CanAccessRoomAsync(string roomName, int userId);
}