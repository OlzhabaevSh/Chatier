using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.NotificationFeatures.Services;
using Chatier.Core.Features.UserFeatures;
using Chatier.Functionals.Configs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Orleans.TestingHost;

namespace DeleteMessages;

[TestClass]
public class Scenario001
{
    private static TestCluster Cluster;

    [TestMethod]
    public async Task UserCanDeleteMessageTest() 
    {
        //// setup
        var alpha = this.CreateUser("alpha");
        var beta = this.CreateUser("beta");

        var messages = new List<(string Username, string Message, bool Remove)>() 
        {
            (alpha.Name, "Hello, Beta! How are you?", false),
            (beta.Name, "Hello, Alpha! I'm good! What about you?", false),
            (alpha.Name, "[incorrect message content! delete]", true),
            (alpha.Name, "I'm good too! See you around!", false),
            (beta.Name, "Have a nice day!", false)
        };

        //// act
        var chatName = await alpha.ChatGrain.CreateChatAsync(
            beta.Name);

        var chat = this.CreateChat(chatName);

        foreach (var (userName, message, remove) in messages)
        {
            var messageId = await chat.Grain.SendMessageAsync(
                userName, 
                message);

            await Task.Delay(200);

            if (remove)
            {
                _ = await chat.Grain.RemoveMessageAsync(
                    messageId,
                    userName);

                await Task.Delay(200);
            }
        }

        //// assert
        var chatMessages = await chat.Grain.GetMessagesAsync();
        var expectedMessages = messages
            .Where(m => !m.Remove)
            .ToArray();

        chatMessages.Count.Should().Be(expectedMessages.Length);
    }

    #region Helpers
    private User CreateUser(string userName) =>
        new User(
            userName, 
            this.CreateUserGrain(userName),
            this.CreateUserChatGrain(userName));

    private IUserGrain CreateUserGrain(string userName) =>
        Cluster.Client.GetGrain<IUserGrain>(userName);

    private IUserChatGrain CreateUserChatGrain(string userName) =>
        Cluster.Client.GetGrain<IUserChatGrain>(userName);

    record User(
        string Name, 
        IUserGrain Grain,
        IUserChatGrain ChatGrain);

    private Chat CreateChat(string chatName) =>
        new Chat(
            chatName,
            this.CreateChatGrain(chatName));

    private IChatGrain CreateChatGrain(string chatName) =>
        Cluster.Client.GetGrain<IChatGrain>(chatName);

    record Chat(string name, IChatGrain Grain);

    #endregion

    #region Silo configuration

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        Cluster = new TestClusterBuilder(1)
            .AddSiloBuilderConfigurator<TestSiloConfigurations>()
            .Build();
        Cluster.Deploy();
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Cluster.StopAllSilos();
    }

    class TestSiloConfigurations : BaseTestSiloConfiguration
    {
        public override void CustomizeLogging(
            ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddFilter<ConsoleLoggerProvider>((category, _) =>
                category.Contains("Chatier"));

            loggingBuilder.AddDebug();
        }

        public override void CustomizeServices(
            IServiceCollection services)
        {
            services.AddScoped<IEmailService, FakeEmailService>();
        }

        public override void CustomizeConfiguration(
            IConfiguration configuration)
        {
            configuration["ReminderIntervalInMilliseconds"] = $"{10000}";
        }
    }

    #endregion
}
