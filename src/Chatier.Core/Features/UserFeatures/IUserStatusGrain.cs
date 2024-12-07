namespace Chatier.Core.Features.UserFeatures;

public interface IUserStatusGrain : IGrainWithStringKey
{
    Task SetStatusAsync(
        bool status);
    Task<bool> GetStatusAsync();
}
