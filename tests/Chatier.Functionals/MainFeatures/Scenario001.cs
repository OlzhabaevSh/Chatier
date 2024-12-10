using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.NotificationFeatures.Services;
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Chatier.Functionals.Configs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Moq;
using Orleans.TestingHost;

namespace MainFeatures;

[TestClass]
public sealed class Scenario001
{
    private static TestCluster Cluster;
    private Mock<IUserMessageNotificationObserver> notificationObserverMock = new Mock<IUserMessageNotificationObserver>();

    [TestMethod]
    public async Task ThreeUsersConversationTest()
    {
        //// setup
        // create Alpha and subscribe to notifications
        var alpha = CreateUser("Alpha");
        await alpha.StatusGrain.SetStatusAsync(true);
        await alpha.NotificationsGrain.SubscribeAsync(
            alpha.NotificationObserver);

        // create Bravo and subscribe to notifications
        var bravo = CreateUser("Bravo");
        await bravo.StatusGrain.SetStatusAsync(true);
        await bravo.NotificationsGrain.SubscribeAsync(
            bravo.NotificationObserver);

        // create Charlie and subscribe to notifications
        var charlie = CreateUser("Charlie");
        await charlie.NotificationsGrain.SubscribeAsync(
            charlie.NotificationObserver);

        // chat
        var chat = CreateChat("a-b-c");

        var messages = new List<(string UserName, string Message)>()
        {
            // greeting
            (alpha.Name, "Hello, UserB!"),
            (bravo.Name, "Hello, Alpha!"),

            (alpha.Name, "How are you?"),
            (bravo.Name, "I'm good! What about you?"),

            (alpha.Name, "I'm good too! See you around!"),
            (bravo.Name, "Have a nice day!")
        };

        //// act
        // add users Alpha and Bravo added to the chat
        await chat.Grain.AddUserAsync(alpha.Name);
        await chat.Grain.AddUserAsync(bravo.Name);

        // users Alpha and Bravo start conversation
        foreach (var (userName, message) in messages)
        {
            await chat.Grain.SendMessageAsync(
                userName,
                message);

            await Task.Delay(200);
        }

        // user Charlie joins the chat
        // and starts conversation
        await charlie.StatusGrain.SetStatusAsync(true);

        await chat.Grain.AddUserAsync(charlie.Name);

        await chat.Grain.SendMessageAsync(
            charlie.Name,
            "Hello there!");

        // user Alpha reply to user C
        await Task.Delay(200);
        await chat.Grain.SendMessageAsync(
            alpha.Name,
            "Hello, Charlie!");

        // user Bravo reply to user C
        await Task.Delay(200);
        await chat.Grain.SendMessageAsync(
            bravo.Name,
            "Hey, Charlie!");

        await Task.Delay(200);

        // set all users offline
        await alpha.StatusGrain.SetStatusAsync(false);
        await bravo.StatusGrain.SetStatusAsync(false);
        await charlie.StatusGrain.SetStatusAsync(false);

        //// assert
        // people amount should be 3: Alpha, Bravo, Charlie
        var chatUsers = await chat.Grain.GetUsersAsync();
        chatUsers.Count().Should().Be(3);

        // total amount of messages is 9
        var chatMessages = await chat.Grain.GetMessagesAsync();
        chatMessages.Count().Should().Be(9);

        // users message notifications
        // Alpha
        var alpha_historyNotifications = await alpha.NotificationsGrain.GetHistoryAsync();
        notificationObserverMock.Verify(
            x => x.ReceiveNotification(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(),
                alpha.Name),
            Times.Exactly(alpha_historyNotifications.Length));

        // Bravo
        var bravo_historyNotifications = await bravo.NotificationsGrain.GetHistoryAsync();
        notificationObserverMock.Verify(
            x => x.ReceiveNotification(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(),
                bravo.Name),
            Times.Exactly(bravo_historyNotifications.Length));

        // Charlie
        var charlie_historyNotifications = await charlie.NotificationsGrain.GetHistoryAsync();
        notificationObserverMock.Verify(
            x => x.ReceiveNotification(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(),
                charlie.Name),
            Times.Exactly(charlie_historyNotifications.Length));
    }

    #region Helpers

    private User CreateUser(
        string userName)
    {
        var userGrain = GetUserGrain(userName);
        var userNotificationsGrain = GetNotificationGrain(userName);
        var userNotificationObserver = GetNotificationObserver(notificationObserverMock.Object);
        var userStatusGrain = GetUserStatusGrain(userName);
        return new User(
            userName,
            userGrain,
            userNotificationsGrain,
            userNotificationObserver,
            userStatusGrain);
    }

    private IUserMessageNotificationObserver GetNotificationObserver(
        IUserMessageNotificationObserver observer) =>
            Cluster.Client.CreateObjectReference<IUserMessageNotificationObserver>(observer);

    private IUserGrain GetUserGrain(string userName) =>
        Cluster.Client.GetGrain<IUserGrain>(userName);

    private IChatGrain GetChatGrain(string chatName) =>
        Cluster.Client.GetGrain<IChatGrain>(chatName);

    private IUserMessageNotificationGrain GetNotificationGrain(string userName) =>
        Cluster.Client.GetGrain<IUserMessageNotificationGrain>(userName);

    private IUserStatusGrain GetUserStatusGrain(string userName) =>
        Cluster.Client.GetGrain<IUserStatusGrain>(userName);

    record User(
        string Name,
        IUserGrain Grain,
        IUserMessageNotificationGrain NotificationsGrain,
        IUserMessageNotificationObserver NotificationObserver,
        IUserStatusGrain StatusGrain);

    private Chat CreateChat(string name) =>
        new Chat(
            name,
            GetChatGrain(name));

    record Chat(
        string Name,
        IChatGrain Grain);

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