using System.Collections.Concurrent;

namespace Chatier.Apps.SignalrService.Services;

public interface IUserSubscriptionQueue
{
    void Enqueue(string userName);
    bool TryDequeue(out string userName);
}

public class UserSubscriptionQueue : IUserSubscriptionQueue
{
    private readonly ConcurrentQueue<string> queue;
    private readonly ILogger<UserSubscriptionQueue> logger;

    public UserSubscriptionQueue(
        ILogger<UserSubscriptionQueue> logger)
    {
        this.queue = new ConcurrentQueue<string>();
        this.logger = logger;
    }

    public void Enqueue(string userName) => 
        this.queue.Enqueue(userName);

    public bool TryDequeue(out string userName) =>
        this.queue.TryDequeue(out userName);
}
