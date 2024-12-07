using Chatier.Core.Features.NotificationFeatures.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Chatier.Core;

public static class ScExtensions
{
    public static IServiceCollection AddChatier(
        this IServiceCollection services)
    {
        services.AddScoped<IEmailService, FakeEmailService>();

        return services;
    }
}
