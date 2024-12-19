using Chatier.Apps.SignalrService.Hubs;
using Chatier.Apps.SignalrService.Services;
using Chatier.Core.Features.ChatFeatures;
using Chatier.Core.Features.NotificationFeatures.Services;
using Chatier.Core.Features.UserFeatures;
using Chatier.Core.Features.UserFeatures.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
builder.Services.AddScoped<IUserService, UserService>();

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

    // configure dashboard
    siloBuilder.UseDashboard(x => 
    {
        x.HostSelf = true;
    });
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

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions = new JsonSerializerOptions(
            JsonSerializerDefaults.Web);
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

app.MapHub<UserHub>("/userHub");

app.Map("/dashboard", x => x.UseOrleansDashboard());

app.Run();