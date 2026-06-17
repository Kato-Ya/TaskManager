using System.Collections.Concurrent;

namespace ChatService.ConnectionManager;
public class ConnectionManager : IConnectionManager
{
    //matching the UserId <--> connectionId
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, byte>> _connections = new();

    public void AddConnection(int userId, string connectionId)
    {
        var userConnections = _connections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, byte>());
        userConnections.TryAdd(connectionId, 0);
    }

    public void RemoveConnection(string connectionId)
    {
        foreach (var (userId, connections) in _connections)
        {
            if (connections.TryRemove(connectionId, out _))
            {
                if (connections.Count == 0)
                {
                    _connections.TryRemove(userId, out _);
                }
                break;
            }
        }
    }

    public bool TryGetConnection(int userId, out IEnumerable<string> connectionIds)
    {
        if (_connections.TryGetValue(userId, out var connectionSet))
        {
            connectionIds = connectionSet.Keys;
            return true;
        }

        connectionIds = Enumerable.Empty<string>();
        return false;

    }

}
