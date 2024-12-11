using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.NotificationFeatures;
using Chatier.Core.Features.UserFeatures;
using Chatier.Functionals.Configs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Orleans.TestingHost;

namespace Notifications;

[TestClass]
public class Scenario001
{
    private static TestCluster Cluster;

    [TestMethod]
    public async Task EmailNotificationSentSuccessfully() 
    {
        //// setup
        // alpha
        var alpha = this.CreateUser("Alpha");
        await alpha.StatusGrain.SetStatusAsync(true);

        // bravo
        var bravo = this.CreateUser("Bravo");
        await bravo.StatusGrain.SetStatusAsync(true);

        //// act
        var chatName = await alpha.ChatGrain.CreateChatAsync(bravo.Name);
        var chat = this.CreateChat(chatName);

        // message naming convention:
        // {sender actorName} + message + {number}:
        // am1: alpha + message + 1
        // bm1: bravo + message + 1
        var am1Id = await chat.Grain.SendMessageAsync(
            alpha.Name,
            "Hey, Bravo!");

        var am1NId = await bravo.Grain.GetLatestMessageNotificationIdAsync();

        await Task.Delay(200);

        await bravo.Grain.ConfirmNotificationAsync(
            am1NId);

        var bm1Id = await chat.Grain.SendMessageAsync(
            bravo.Name,
            "Hello, Alpha!");

        var bm1NId = await alpha.Grain.GetLatestMessageNotificationIdAsync();

        await Task.Delay(200);

        await alpha.Grain.ConfirmNotificationAsync(
            bm1NId);

        await bravo.StatusGrain.SetStatusAsync(false);

        await Task.Delay(200);

        var am2Id = await chat.Grain.SendMessageAsync(
            alpha.Name,
            "Are you there, Bravo?");

        var am2NId = await bravo.Grain.GetLatestMessageNotificationIdAsync();

        await Task.Delay(5_000);

        //// assert
        var alphaStatus = await alpha.StatusGrain.GetStatusAsync();
        alphaStatus.Should().BeTrue(
            because: $"'{alpha.Name}' is online");

        var bravoStatus = await bravo.StatusGrain.GetStatusAsync();
        bravoStatus.Should().BeFalse(
            because: $"'{bravo.Name}' is offline");

        var am1Notification = this.GetNotificationGrain(am1NId);
        var am1WasSent = await am1Notification.SentExternally();
        am1WasSent.Should().BeFalse(
            because: $"'{am1NId}' was called by '{bravo.Name}' via ConfirmNotificationAsync");

        var bm1Notification = this.GetNotificationGrain(bm1NId);
        var bm1WasSent = await bm1Notification.SentExternally();
        bm1WasSent.Should().BeFalse(
            because: $"'{bm1NId}' was called by '{alpha.Name}' via ConfirmNotificationAsync");

        var am2Notification = this.GetNotificationGrain(am2NId);
        var am2WasSent = await am2Notification.SentExternally();
        am2WasSent.Should().BeTrue(
            because: $"'{am2NId}' wasn't called by '{bravo.Name}' SendMessageAsync and '{bravo.Name}' is offline");
    }

    #region Helpers
    private User CreateUser(string userName) =>
        new User(
            userName,
            this.GetUserGrain(userName),
            this.GetNotificationGrain(userName),
            this.GetUserStatusGrain(userName),
            this.GetUserChatGrain(userName));

    private IUserGrain GetUserGrain(string userName) =>
        Cluster.Client.GetGrain<IUserGrain>(userName);

    private IUserMessageNotificationGrain GetNotificationGrain(string userName) =>
        Cluster.Client.GetGrain<IUserMessageNotificationGrain>(userName);

    private IUserStatusGrain GetUserStatusGrain(string userName) =>
        Cluster.Client.GetGrain<IUserStatusGrain>(userName);

    private IUserChatGrain GetUserChatGrain(string userName) =>
        Cluster.Client.GetGrain<IUserChatGrain>(userName);

    record User(
        string Name, 
        IUserGrain Grain,
        IUserMessageNotificationGrain NotificationGrain,
        IUserStatusGrain StatusGrain,
        IUserChatGrain ChatGrain);

    private Chat CreateChat(string chatName) =>
        new Chat(
            chatName,
            this.GetChatGrain(chatName));

    private IChatGrain GetChatGrain(string chatName) =>
        Cluster.Client.GetGrain<IChatGrain>(chatName);

    record Chat(
        string name, 
        IChatGrain Grain);

    private INotificationGrain GetNotificationGrain(Guid messageId) =>
        Cluster.Client.GetGrain<INotificationGrain>(messageId);

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

    #endregion
}
