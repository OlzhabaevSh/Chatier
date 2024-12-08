using System.Collections.Concurrent;

namespace Chatier.Apps.SignalrService.Services;

public interface IHubGroupStore
{
    Task AddToGroupAsync(string groupName, string connectionId);
    Task RemoveFromGroupAsync(string groupName, string connectionId);
    Task<IEnumerable<string>> GetConnectionsInGroupAsync(string groupName);
    Task<bool> IsGroupHasConnections(string groupName);
}

public class HubGroupStore : IHubGroupStore
{
    private readonly ConcurrentDictionary<string, HashSet<string>> groupConnections = new();
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    public async Task AddToGroupAsync(string groupName, string connectionId)
    {
        await this.semaphore.WaitAsync();
        try
        {
            if (!this.groupConnections.TryGetValue(groupName, out var connections))
            {
                connections = new HashSet<string>();
                this.groupConnections[groupName] = connections;
            }
            connections.Add(connectionId);
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    public async Task RemoveFromGroupAsync(string groupName, string connectionId)
    {
        await this.semaphore.WaitAsync();
        try
        {
            if (this.groupConnections.TryGetValue(groupName, out var connections))
            {
                connections.Remove(connectionId);
            }
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    public async Task<IEnumerable<string>> GetConnectionsInGroupAsync(string groupName)
    {
        await this.semaphore.WaitAsync();
        try
        {
            if (this.groupConnections.TryGetValue(groupName, out var connections))
            {
                return connections;
            }
            return Enumerable.Empty<string>();
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    public async Task<bool> IsGroupHasConnections(string groupName)
    {
        var data = await GetConnectionsInGroupAsync(groupName);
        return data.Any();
    }
}
