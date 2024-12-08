using System.Collections.Concurrent;

namespace Chatier.Apps.SignalrService.Services;

public interface IUserUnSubscriptionQueue
{
    void Enqueue(string userName);
    bool TryDequeue(out string userName);
}

public class UserUnSubscriptionQueue : IUserUnSubscriptionQueue
{
    private readonly ConcurrentQueue<string> queue;
    private readonly ILogger<UserUnSubscriptionQueue> logger;

    public UserUnSubscriptionQueue(
        ILogger<UserUnSubscriptionQueue> logger)
    {
        this.queue = new ConcurrentQueue<string>();
        this.logger = logger;
    }

    public void Enqueue(string userName) =>
        this.queue.Enqueue(userName);

    public bool TryDequeue(out string userName) =>
        this.queue.TryDequeue(out userName);
}
