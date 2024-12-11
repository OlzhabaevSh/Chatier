using Chatier.Apps.SignalrService.Hubs;
using Chatier.Apps.SignalrService.Services;
using Chatier.Core.Features.UserFeatures.Services;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHub<UserHub>("/userHub");

app.Run();