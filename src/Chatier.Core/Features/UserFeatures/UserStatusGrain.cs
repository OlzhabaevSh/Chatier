namespace Chatier.Core.Features.UserFeatures;

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