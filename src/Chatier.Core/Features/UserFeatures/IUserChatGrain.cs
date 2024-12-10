using Chatier.Core.Features.ChatFeatures;

namespace Chatier.Core.Features.UserFeatures;

#region Interfaces
public interface IUserChatGrain : IGrainWithStringKey
{
    Task<UserChatItem[]> GetChatsAsync();

    Task CreateOwnChatAsync();

    Task<string> CreateChatAsync(string userName);

    Task CreateGroupChatAsync(string groupName);

    Task AddUserToGroupChatAsync(
        string groupName, 
        string userName);

    Task IncludeToGroupAsync(
        string groupName, 
        string sender);
}
#endregion

#region Implementation
public class UserChatGrain : Grain, IUserChatGrain
{
    private readonly IPersistentState<UserChatGrainState> state;

    public UserChatGrain(
        [PersistentState("userChats", "userStore")] 
        IPersistentState<UserChatGrainState> state)
    {
        this.state = state;
    }

    public Task<UserChatItem[]> GetChatsAsync() 
    {
        var data = this.state.State.Chats.Values.ToArray();
        return Task.FromResult(data);
    }

    public async Task CreateOwnChatAsync()
    {
        var name = this.GetPrimaryKeyString();
        if (this.state.State.Chats.ContainsKey(name))
        {
            return;
        }

        this.state.State.Chats.Add(name, new UserChatItem
        {
            Name = name,
            FriendlyName = name,
            ChatType = UserChatEnum.Own,
            Owner = name
        });

        await this.state.WriteStateAsync();

        var chatGrain = this.GrainFactory.GetGrain<IChatGrain>(name);
        await chatGrain.AddUserAsync(name);
    }

    public async Task<string> CreateChatAsync(string userName)
    {
        var myName = this.GetPrimaryKeyString();

        var names = new string[] 
        {
            myName,
            userName
        };

        var chatName = string.Join("-", names.OrderBy(x => x));

        if (this.state.State.Chats.ContainsKey(chatName))
        {
            return chatName;
        }

        this.state.State.Chats.Add(chatName, new UserChatItem
        {
            Name = chatName,
            FriendlyName = userName,
            ChatType = UserChatEnum.Chat,
            Owner = myName
        });

        await this.state.WriteStateAsync();

        var chatGrain = this.GrainFactory.GetGrain<IChatGrain>(chatName);
        await chatGrain.AddUserAsync(myName);

        await this.AddUserToGroupChatAsync(chatName, userName);

        return chatName;
    }

    public async Task CreateGroupChatAsync(string groupName)
    {
        var myName = this.GetPrimaryKeyString();

        if(this.state.State.Chats.ContainsKey(groupName))
        {
            return;
        }

        this.state.State.Chats.Add(groupName, new UserChatItem
        {
            Name = groupName,
            FriendlyName = groupName,
            ChatType = UserChatEnum.Group,
            Owner = myName
        });

        await this.state.WriteStateAsync();

        var chatGrain = this.GrainFactory.GetGrain<IChatGrain>(groupName);
        await chatGrain.AddUserAsync(myName);
    }

    public async Task AddUserToGroupChatAsync(
        string groupName, 
        string userName)
    {
        if (!this.state.State.Chats.ContainsKey(groupName))
        {
            return;
        }

        var myName = this.GetPrimaryKeyString();

        var userChatGrain = this.GrainFactory.GetGrain<IUserChatGrain>(userName);

        await userChatGrain.IncludeToGroupAsync(groupName, myName);
    }

    public async Task IncludeToGroupAsync(
        string groupName,
        string sender) 
    {
        if(this.state.State.Chats.ContainsKey(groupName))
        {
            return;
        }

        this.state.State.Chats.Add(groupName, new UserChatItem
        {
            Name = groupName,
            FriendlyName = sender,
            ChatType = UserChatEnum.Group,
            Owner = sender
        });

        var myName = this.GetPrimaryKeyString();

        var chatGrain = this.GrainFactory.GetGrain<IChatGrain>(groupName);
        await chatGrain.AddUserAsync(myName);
    }
}
#endregion

#region State model
[GenerateSerializer]
public class UserChatGrainState
{
    [Id(0)]
    public Dictionary<string, UserChatItem> Chats { get; set; } = new Dictionary<string, UserChatItem>();
}

[GenerateSerializer]
public class UserChatItem 
{
    [Id(0)]
    public required string Name { get; init; }

    [Id(1)]
    public required string FriendlyName { get; set; }

    [Id(2)]
    public UserChatEnum ChatType { get; set; }

    [Id(3)]
    public required string Owner { get; init; }
}

public enum UserChatEnum
{
    Own,
    Chat,
    Group
}
#endregion