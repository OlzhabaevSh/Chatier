using Chatier.Core.Features.UserFeatures;
using Microsoft.Extensions.Logging;

namespace Chatier.Core.Features.ChatFeatures;

public class ChatGrain : Grain, IChatGrain
{
    private readonly IPersistentState<ChatGrainUsersState> usersState;
    private readonly IPersistentState<ChatGrainMessagesState> messagesState;

    private readonly ILogger<ChatGrain> logger;

    public ChatGrain(
        [PersistentState("users", "chatStore")]
        IPersistentState<ChatGrainUsersState> usersState,
        [PersistentState("messages", "chatStore")]
        IPersistentState<ChatGrainMessagesState> messagesState,
        ILogger<ChatGrain> logger)
    {
        this.usersState = usersState;
        this.messagesState = messagesState;
        this.logger = logger;
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

            return;
        }

        this.usersState.State.Users.Add(userName);
        await this.usersState.WriteStateAsync();

        var users = this.usersState.State.Users;
        foreach (var user in users)
        {
            var userGrain = this.GrainFactory.GetGrain<IUserGrain>(user);
            await userGrain.NotifyAboutAddingToChatAsync(
                chat: this.GetPrimaryKeyString(),
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
            await userGrain.NotifyAboutLeavingAChatAsync(
                chat: this.GetPrimaryKeyString(),
                userName: userName);
        }
    }

    public Task<Dictionary<Guid, ChatMessageItem>> GetMessagesAsync()
    {
        var messages = this.messagesState.State.Messages;
        return Task.FromResult(messages);
    }

    public async Task<Guid> SendMessageAsync(
        string userName,
        string message)
    {
        var messageId = Guid.NewGuid();

        this.messagesState.State.Messages.Add(
            key: messageId,
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
            await userGrain.NotifyAboutNewMessageAsync(
                chat: this.GetPrimaryKeyString(),
                sender: userName,
                messageId: messageId,
                message: message);
        }

        return messageId;
    }

    public async Task<bool> RemoveMessageAsync(
        Guid messageId,
        string caller)
    {
        if (!this.messagesState.State.Messages.ContainsKey(messageId))
        {
            return false;
        }

        if (this.messagesState.State.Messages[messageId].Sender != caller)
        {
            return false;
        }

        this.messagesState.State.Messages.Remove(messageId);
        await this.messagesState.WriteStateAsync();

        return true;
    }
}