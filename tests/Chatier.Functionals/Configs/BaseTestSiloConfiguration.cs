using Chatier.Core.Features.NotificationFeatures.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Orleans.TestingHost;

namespace Chatier.Functionals.Configs;

public class BaseTestSiloConfiguration : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureLogging(loggingBuilder =>
        {
            this.CustomizeLogging(loggingBuilder);
        });

        this.CustomizeServices(siloBuilder.Services);

        // configurations
        this.CustomizeConfiguration(siloBuilder.Configuration);
        
        // configure grain storage
        siloBuilder.AddMemoryGrainStorage("chatStore");
        siloBuilder.AddMemoryGrainStorage("notificationStore");
        siloBuilder.AddMemoryGrainStorage("userStore");
        siloBuilder.AddMemoryGrainStorage("userNotifications");
        siloBuilder.AddMemoryGrainStorage("PubSubStore");

        // configure streams
        siloBuilder.AddMemoryStreams("MemoryStreamProvider");
        // configure reminder service
        siloBuilder.UseInMemoryReminderService();
    }

    public virtual void CustomizeLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddFilter<ConsoleLoggerProvider>((category, _) => 
            category.Contains("Chatier"));
        loggingBuilder.AddDebug();
    }

    public virtual void CustomizeServices(
        IServiceCollection services)
    {
        services.AddScoped<IEmailService, FakeEmailService>();
    }

    public virtual void CustomizeConfiguration(IConfiguration configuration) 
    {
        configuration["ReminderIntervalInMilliseconds"] = $"{10000}";
    }
}
