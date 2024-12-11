using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;

namespace Chatier.Apps.SignalrService.Services;

public interface IUserNotificationChannel 
{
    Task SubscribeAsync(
        string userName,
        string connectionId);

    Task UnSubscribeAsync(
        string userName,
        string connectionId);

    IAsyncEnumerable<UserSubscriberModel> GetSubscriptionEnumerableAsync(
        CancellationToken stoppingToken);

    IAsyncEnumerable<UserUnSubscriberModel> GetUnSubscriptionEnumerableAsync(
        CancellationToken stoppingToken);
}

public class UserNotificationChannel : IUserNotificationChannel
{
    private readonly ConcurrentDictionary<string, HashSet<string>> groups = new();
    private Channel<UserSubscriberModel> subscriberChannel;
    private Channel<UserUnSubscriberModel> unSubscriberChannel;

    public UserNotificationChannel()
    {
        this.subscriberChannel = Channel.CreateBounded<UserSubscriberModel>(100);
        this.unSubscriberChannel = Channel.CreateBounded<UserUnSubscriberModel>(100);
    }

    public async Task SubscribeAsync(
        string userName,
        string connectionId)
    {
        if (this.groups.TryGetValue(userName, out HashSet<string> connections)) 
        {
            connections.Add(connectionId);
        }
        else
        {
            connections = new HashSet<string> { connectionId };
            this.groups.TryAdd(userName, connections);

            await this.subscriberChannel.Writer.WriteAsync(
                new UserSubscriberModel(userName));
        }
    }

    public async Task UnSubscribeAsync(
        string userName,
        string connectionId)
    {
        if (!this.groups.ContainsKey(userName)) 
        {
            return;
        }

        if (this.groups.TryGetValue(userName, out HashSet<string> connections))
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                this.groups.TryRemove(userName, out _);
                await this.unSubscriberChannel.Writer.WriteAsync(
                    new UserUnSubscriberModel(userName));
            }
        }
    }

    public IAsyncEnumerable<UserSubscriberModel> GetSubscriptionEnumerableAsync(
        CancellationToken cancellationToken) 
    {
        var result = this.subscriberChannel.Reader.ReadAllAsync(cancellationToken);
        return result;
    }

    public IAsyncEnumerable<UserUnSubscriberModel> GetUnSubscriptionEnumerableAsync(
        CancellationToken stoppingToken) 
    {
        var result = this.unSubscriberChannel.Reader.ReadAllAsync(stoppingToken);
        return result;
    }
}

public record UserSubscriberModel(string userName);

public record UserUnSubscriberModel(string userName);