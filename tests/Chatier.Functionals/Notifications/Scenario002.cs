using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Chatier.Functionals.Configs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Orleans.TestingHost;

namespace Notifications;

[TestClass]
public class Scenario002
{
    private static TestCluster Cluster;
    private TestUserChatNotificationObserver observer = new();

    [TestMethod]
    public async Task AddingToGroupSendsNotifications() 
    {
        //// setup
        // alpha
        var alpha = this.CreateUser("Alpha");
        await alpha.ChatNotificationGrain.SubscribeAsync(
            alpha.ChatNotificationObserver);

        // bravo
        var bravo = this.CreateUser("Bravo");
        await bravo.ChatNotificationGrain.SubscribeAsync(
            bravo.ChatNotificationObserver);

        //// act
        // alpha own chat
        await alpha.ChatGrain.CreateOwnChatAsync();

        var alphaOwnGroupNotification = observer.GetLastNotification();
        alphaOwnGroupNotification.ChatName.Should().Be(alpha.Name);

        this.observer.ClearNotifications();

        // bravo own chat
        await bravo.ChatGrain.CreateOwnChatAsync();
        var bravoOwnGroupNotification = observer.GetLastNotification();
        bravoOwnGroupNotification.ChatName.Should().Be(bravo.Name);

        this.observer.ClearNotifications();

        // alpha add bravo to chat
        var alphaBravoChatName = await alpha.ChatGrain.CreateChatAsync(bravo.Name);
        var alphaBravoGroupNotification = observer.GetNotifications();

        alphaBravoGroupNotification.Should()
            .HaveCount(
                expected: 3,
                because: "Alpha added to the chat, Bravo added to the chat, Alpha was notified about Bravo");

        //// assert
    }

    #region Helpers
    private User CreateUser(string name) =>
        new User(
            name,
            this.GetUserGrain(name),
            this.GetUserChatGrain(name),
            this.GetUserChatNotificationGrain(name),
            this.GetChatNotificationObserver(this.observer));

    private IUserGrain GetUserGrain(string name) =>
        Cluster.Client.GetGrain<IUserGrain>(name);

    private IUserChatGrain GetUserChatGrain(string name) =>
        Cluster.Client.GetGrain<IUserChatGrain>(name);

    private IUserChatNotificationGrain GetUserChatNotificationGrain(string name) =>
        Cluster.Client.GetGrain<IUserChatNotificationGrain>(name);

    private IUserChatNotificationObserver GetChatNotificationObserver(
        IUserChatNotificationObserver observer) =>
            Cluster.Client.CreateObjectReference<IUserChatNotificationObserver>(observer);

    record User(
        string Name,
        IUserGrain UserGrain,
        IUserChatGrain ChatGrain,
        IUserChatNotificationGrain ChatNotificationGrain,
        IUserChatNotificationObserver ChatNotificationObserver);
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
        public override void CustomizeConfiguration(
            IConfiguration configuration)
        {
            configuration["ReminderIntervalInMilliseconds"] = $"{1_000}";
        }
    }

    class TestUserChatNotificationObserver : IUserChatNotificationObserver
    {
        private readonly List<NotificationModel> notifications = new();

        public NotificationModel GetLastNotification() =>
            notifications.LastOrDefault()!;

        public void ClearNotifications() =>
            notifications.Clear();

        public NotificationModel[] GetNotifications() =>
            notifications.ToArray();

        public Task ReceiveNotification(
            Guid notificationId, 
            string chatName, 
            string userName, 
            UserGroupNotificationType notificationType, 
            DateTimeOffset createdAt, 
            string receiverName)
        {
            notifications.Add(new NotificationModel
            {
                NotificationId = notificationId,
                ChatName = chatName,
                UserName = userName,
                NotificationType = notificationType,
                CreatedAt = createdAt,
                ReceiverName = receiverName
            });

            return Task.CompletedTask;
        }
    }

    class NotificationModel 
    {
        public Guid NotificationId { get; init; }
        public required string ChatName { get; init; }
        public required string UserName { get; init; }
        public UserGroupNotificationType NotificationType { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public required string ReceiverName { get; init; }
    }
    #endregion
}
