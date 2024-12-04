using Chatier.Core.Features.UserFeatures;
using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.ChatFeatures;

public class ChatGrain : Grain, IChatGrain
{
    private readonly IPersistentState<ChatUsersState> usersState;
    private readonly IPersistentState<ChatMessagesState> messagesState;

    private readonly ILogger<ChatGrain> logger;

    public ChatGrain(
        [PersistentState("users", "chatStore")]
        IPersistentState<ChatUsersState> usersState,
        [PersistentState("messages", "chatStore")]
        IPersistentState<ChatMessagesState> messagesState,
        ILogger<ChatGrain> logger)
    {
        this.usersState = usersState;
        this.messagesState = messagesState;
        this.logger = logger;
    }

    public Task<string> GetNameAsync()
    {
        var name = this.GetPrimaryKeyString();
        return Task.FromResult(name);
    }

    public Task<string[]> GetUsersAsync()
    {
        var users = this.usersState.State.Users;
        return Task.FromResult(users.ToArray());
    }

    public async Task AddUserAsync(
        string userName)
    {
        if (this.usersState.State.Users.Contains(userName))
        {
            var grainName = this.GetPrimaryKeyString();
            this.logger.LogWarning(
                "Grain with name '{grainName}' already contains a user with name '{userName}'",
                grainName,
                userName);
        }

        this.usersState.State.Users.Add(userName);
        await this.usersState.WriteStateAsync();

        var users = this.usersState.State.Users;
        foreach (var user in users)
        {
            var userGrain = this.GrainFactory.GetGrain<IUserGrain>(user);
            await userGrain.NotifyAboutAddingToGroupAsync(
                groupName: this.GetPrimaryKeyString(),
                userName: userName);
        }
    }

    public async Task RemoveUserAsync(
        string userName)
    {
        if (!this.usersState.State.Users.Contains(userName))
        {
            var grainName = this.GetPrimaryKeyString();
            this.logger.LogWarning(
                "Grain with name '{grainName}' does not contain a user with name '{userName}'",
                grainName,
                userName);
        }

        this.usersState.State.Users.Remove(userName);
        await this.usersState.WriteStateAsync();

        var users = this.usersState.State.Users;
        foreach (var user in users)
        {
            var userGrain = this.GrainFactory.GetGrain<IUserGrain>(user);
            await userGrain.NotifyAboutLeavingAGroupAsync(
                groupName: this.GetPrimaryKeyString(),
                userName: userName);
        }
    }

    public Task<Dictionary<Guid, ChatMessageItem>> GetMessagesAsync()
    {
        var messages = this.messagesState.State.Messages;
        return Task.FromResult(messages);
    }

    public async Task SendMessageAsync(
        string userName,
        string message)
    {
        this.messagesState.State.Messages.Add(
            key: Guid.NewGuid(),
            value: new ChatMessageItem()
            {
                Sender = userName, 
                Message = message, 
                CreatedAt = DateTimeOffset.UtcNow 
            });

        await this.messagesState.WriteStateAsync();

        var users = this.usersState.State.Users;
        foreach (var user in users)
        {
            var userGrain = this.GrainFactory.GetGrain<IUserGrain>(user);
            await userGrain.NotifyAboutMessageAsync(
                groupName: this.GetPrimaryKeyString(),
                sender: userName,
                messageId: Guid.NewGuid(),
                message: message);
        }
    }
}