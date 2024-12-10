namespace Chatier.Core.Features.UserFeatures;

#region Interface
public interface IUserStatusGrain : IGrainWithStringKey
{
    Task SetStatusAsync(
        bool status);
    Task<bool> GetStatusAsync();
}
#endregion

#region Implementation
public class UserStatusGrain : IGrain, IUserStatusGrain
{
    private readonly IPersistentState<UserStatusGrainState> state;
    public UserStatusGrain(
        [PersistentState("userStatuses", "userStore")]
        IPersistentState<UserStatusGrainState> state)
    {
        this.state = state;
    }

    public async Task SetStatusAsync(
        bool status)
    {
        if (this.state.State.Online == status)
        {
            return;
        }

        this.state.State.Online = status;
        this.state.State.LastVisitedAt = DateTimeOffset.UtcNow;
        await this.state.WriteStateAsync();
    }

    public Task<bool> GetStatusAsync() =>
        Task.FromResult(this.state.State.Online);
}
#endregion

#region State model
[GenerateSerializer]
public class UserStatusGrainState
{
    public bool Online { get; set; }
    public DateTimeOffset LastVisitedAt { get; set; }
}
#endregion