using Chatier.Apps.SignalrService.Hubs;
using Chatier.Apps.SignalrService.Services;
using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.NotificationFeatures.Services;
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Microsoft.AspNetCore.Mvc;
using Orleans;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

builder.Services.AddHostedService<NotificationBackgroundService>();

// services
builder.Services.AddSingleton<IUserMessageNotificationObserver, SignalrUserMessageNotificationObserver>();
builder.Services.AddSingleton<IUserChatNotificationObserver, SignalrUserChatNotificationObserver>();
builder.Services.AddSingleton<IUserNotificationChannel, UserNotificationChannel>();

builder.Services.AddScoped<IEmailService, FakeEmailService>();

builder.Host.UseOrleans(static siloBuilder => 
{
    siloBuilder.UseLocalhostClustering();

    // configure grain storage
    siloBuilder.AddMemoryGrainStorage("chatStore");
    siloBuilder.AddMemoryGrainStorage("notificationStore");
    siloBuilder.AddMemoryGrainStorage("userStore");
    siloBuilder.AddMemoryGrainStorage("userNotifications");
    siloBuilder.AddMemoryGrainStorage("PubSubStore");
    siloBuilder.AddMemoryGrainStorage("UserGroupState");

    // configure streams
    siloBuilder.AddMemoryStreams("MemoryStreamProvider");
    // configure reminder service
    siloBuilder.UseInMemoryReminderService();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSignalR();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

app.MapHub<UserHub>("/userHub");

var apiGroup = app.MapGroup("/api");

apiGroup.MapGet("/createUsers", async([FromServices] IClusterClient clusterClient) => 
{
    var alphaGrain = clusterClient.GetGrain<IUserGrain>("alpha");
    var betaGrain = clusterClient.GetGrain<IUserGrain>("beta");

    var alphChatGrain = clusterClient.GetGrain<IUserChatGrain>("alpha");
    var chatName = await alphChatGrain.CreateChatAsync("beta");

    return Results.Ok(chatName);
});

apiGroup.MapGet(
    "/send/{fromUser}/to/{toUser}/{message}",
    async (
        [FromQuery] string fromUser,
        [FromQuery] string toUser,
        [FromQuery] string message,
        [FromServices] IClusterClient clusterClient) => 
    {
        var names = new[] { fromUser, toUser };
        var chatName = string.Join("-", names.OrderBy(n => n));
        var chatGrain = clusterClient.GetGrain<IChatGrain>(chatName);

        await chatGrain.SendMessageAsync(fromUser, message);

        return Results.Ok();
    });

app.Run();