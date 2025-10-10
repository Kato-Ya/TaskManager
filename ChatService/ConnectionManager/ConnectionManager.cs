using System.Collections.Concurrent;

namespace ChatService.ConnectionManager;
public class ConnectionManager : IConnectionManager
{
    //matching the UserId <--> connectionId
    private readonly ConcurrentDictionary<int, HashSet<string>> _connections = new();

    public void AddConnection(int userId, string connectionId)
    {
        _connections.AddOrUpdate(
            userId, _ => new HashSet<string> {connectionId}, (_, existing) =>
        {
            existing.Add(connectionId);
            return existing;
        });
    }

    public void RemoveConnection(string connectionId)
    {
        foreach (var (userId, connections) in _connections)
        {
            if (connections.Contains(connectionId))
            {
                connections.Remove(connectionId);

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
            connectionIds = connectionSet;
            return true;
        }

        connectionIds = Enumerable.Empty<string>();
        return false;

    }

}
