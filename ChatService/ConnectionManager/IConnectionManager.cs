namespace ChatService.ConnectionManager;
public interface IConnectionManager
{
    void AddConnection(int userId, string connectionId);
    void RemoveConnection(string connectionId);
    bool TryGetConnection(int userId, out IEnumerable<string> connectionIds);

}
