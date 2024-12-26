using Chatier.Apps.SignalrService.Hubs;
using Chatier.Apps.SignalrService.Services;
using Chatier.Core.Features.UserFeatures.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSignalR();

builder.Services.AddHostedService<NotificationBackgroundService>();

// services
builder.Services.AddSingleton<IUserMessageNotificationObserver, SignalrUserMessageNotificationObserver>();
builder.Services.AddSingleton<IUserChatNotificationObserver, SignalrUserChatNotificationObserver>();
builder.Services.AddSingleton<IUserNotificationChannel, UserNotificationChannel>();

// builder.Services.AddScoped<IEmailService, FakeEmailService>();
builder.Services.AddScoped<IUserService, UserService>();

// setup orleans client
// clustering
builder.AddKeyedRedisClient("chatierClusteringRedis");
builder.AddKeyedRedisClient("chatierGrainStorageRedis");
builder.AddKeyedRedisClient("chatierSystemRedis");
// orleans
builder.UseOrleansClient();

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

// app.Map("/dashboard", x => x.UseOrleansDashboard());

app.Run();