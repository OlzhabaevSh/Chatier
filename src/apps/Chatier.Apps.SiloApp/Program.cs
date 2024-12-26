using Chatier.Core.Features.NotificationFeatures.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IEmailService, FakeEmailService>();

builder.AddServiceDefaults();

// setup orleans silo
// clustering
builder.AddKeyedRedisClient("chatierClusteringRedis");
builder.AddKeyedRedisClient("chatierGrainStorageRedis");
builder.AddKeyedRedisClient("chatierSystemRedis");

builder.UseOrleans(silo => 
{
    silo.UseDashboard(x => x.HostSelf = true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.Map("/dashboard", x => x.UseOrleansDashboard());

await app.RunAsync();