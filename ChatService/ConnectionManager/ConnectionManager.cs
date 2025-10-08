using System.Collections.Concurrent;

namespace ChatService.ConnectionManager;
public class ConnectionManager : IConnectionManager
{
    //matching the UserId <--> connectionId
    private readonly ConcurrentDictionary<int, string> _connections = new();

    public void AddConnection(int userId, string connectionId)
    {
        _connections[userId] = connectionId;
    }

    public void RemoveConnection(string connectionId)
    {
        var user = _connections.FirstOrDefault(x => x.Value == connectionId).Key;
        if (user != 0)
        {
            _connections.TryRemove(user, out _);
        }
    }

    public bool TryGetConnection(int userId, out string connectionId)
    {
        return _connections.TryGetValue(userId, out connectionId!);
    }
}
