using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.UserFeatures;
using FluentAssertions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging;
using Orleans.TestingHost;

namespace FunctionalsTests;

[TestClass]
public sealed class MainScenarioTest
{
    private static TestCluster Cluster;

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

    [TestMethod]
    public async Task ConversationBetweenUsersTest()
    {
        // setup
        var userA = (name: "userA", grain: GetUser("userA"));
        var userB = (name: "userB", grain: GetUser("userB"));
        var userC = (name: "userC", grain: GetUser("userC"));

        var chat = (name: "a-b-c", grain: GetChat("a-b-c"));

        var messages = new List<(string user, string message)>() 
        {
            // greeting
            (userA.name, "Hello, UserB!"),
            (userB.name, "Hello, UserA!"),

            (userA.name, "How are you?"),
            (userB.name, "I'm good! What about you?"),

            (userA.name, "I'm good too! See you around!"),
            (userB.name, "Have a nice day!")
        };

        // act
        // users A and B join the chat
        // and start conversation
        await chat.grain.AddUserAsync(userA.name);
        await chat.grain.AddUserAsync(userB.name);

        foreach (var (userName, message) in messages)
        {
            var userGrain = GetUser(userName);

            var lastMessageNotification = await userGrain.GetLatestMessageNotification();

            if (lastMessageNotification.id != null)
            {
                await userGrain.ConfirmNotificationAsync(lastMessageNotification.id.Value);
            }

            await chat.grain.SendMessageAsync(
                userName, 
                message);
        }

        // user C joins the chat
        // and starts conversation
        await chat.grain.AddUserAsync(userC.name);

        await chat.grain.SendMessageAsync(
            userC.name, 
            "Hello there!");

        var userALastNotification = await userA.grain.GetLatestMessageNotification();
        await userA.grain.ConfirmNotificationAsync(userALastNotification.id.Value);

        var userBLastNotification = await userB.grain.GetLatestMessageNotification();
        await userB.grain.ConfirmNotificationAsync(userBLastNotification.id.Value);

        await Task.Delay(5000);

        await chat.grain.SendMessageAsync(
            userA.name, 
            "Hello, UserC!");

        await Task.Delay(5000);

        await chat.grain.SendMessageAsync(
            userB.name, 
            "Hello, UserC!");

        await Task.Delay(5000);

        // assert
        // people amount should be 3: userA, userB, userC
        var chatUsers = await chat.grain.GetUsersAsync();
        chatUsers.Count().Should().Be(3);

        // total amount of messages is 9
        var chatMessages = await chat.grain.GetMessagesAsync();
        chatMessages.Count().Should().Be(9);

        // userA cases
        // group notifications should be 3
        // because userA was the first the chat and saw userB and userC joining (and himself)
        var userATotalGroupNotifications = await userA.grain.GetAllGroupNotifications();
        userATotalGroupNotifications.Count().Should().Be(3);

        // chat notifications should be 5
        // messages from conversations:
        // 1. userA <-> userB
        // 2. userA, userB <-> userC
        var userATotalToChatNotifications = await userA.grain.GetAllMessageNotifications();
        userATotalToChatNotifications.Count().Should().Be(5);

        // userA should have 8 notifications
        // 3: group notification
        // 5: chat notification
        var userARecievedNotifications = await userA.grain.GetReceivedNotificationsAsync();
        userARecievedNotifications.Count().Should().Be(8);

        // userB cases
        // group notifications should be 2
        // because userB was the second the chat and saw userC joining (and himself)
        var userBTotalGroupNotifications = await userB.grain.GetAllGroupNotifications();
        userBTotalGroupNotifications.Count().Should().Be(2);

        // chat notifications should be 5
        // messages from conversations:
        // 1. userA <-> userB
        // 2. userA, userB <-> userC
        var userBTotalChatNotifications = await userB.grain.GetAllMessageNotifications();
        userBTotalChatNotifications.Count().Should().Be(5);

        // userB should have 7 notifications
        // 2: group notification
        // 5: chat notification
        var userBRecievedNotifications = await userB.grain.GetReceivedNotificationsAsync();
        userBRecievedNotifications.Count().Should().Be(7);

        // userC cases
        var userCTotalNotifications = await userC.grain.GetAllNotifications();

        // group notifications should be 1
        // because userC was the last the chat and saw no one else joining (and himself)
        var userCTotalGroupNotifications = await userC.grain.GetAllGroupNotifications();
        userCTotalGroupNotifications.Count().Should().Be(1);

        // chat notifications should be 2
        // messages from conversations:
        // 1. userA, userB <-> userC
        var userCTotalChatNotifications = await userC.grain.GetAllMessageNotifications();
        userCTotalChatNotifications.Count().Should().Be(2);

        // userC should have 3 notifications
        // 1: group notification
        // 2: chat notification
        var userCRecievedNotifications = await userC.grain.GetReceivedNotificationsAsync();
        userCRecievedNotifications.Count().Should().Be(3);
    }

    private IUserGrain GetUser(string name) =>
        Cluster.GrainFactory.GetGrain<IUserGrain>(name);

    private IChatGrain GetChat(string name) =>
        Cluster.GrainFactory.GetGrain<IChatGrain>(name);

    class TestSiloConfigurations : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddFilter<ConsoleLoggerProvider>((category, _) => category.Contains("Chatier"));

                loggingBuilder.AddDebug();
            });

            // configurations
            siloBuilder.Configuration["ReminderIntervalInMilliseconds"] = $"{10000}";

            // configure grain storage
            siloBuilder.AddMemoryGrainStorage("chatStore");
            siloBuilder.AddMemoryGrainStorage("notificationStore");
            siloBuilder.AddMemoryGrainStorage("userStore");

            // configure reminder service
            siloBuilder.UseInMemoryReminderService();
        }
    }
}
