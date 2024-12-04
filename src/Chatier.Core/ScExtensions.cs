using Microsoft.Extensions.DependencyInjection;

namespace Chatier.Core;

public static class ScExtensions
{
    public static IServiceCollection AddChatier(
        this IServiceCollection services)
    {
        return services;
    }
}
