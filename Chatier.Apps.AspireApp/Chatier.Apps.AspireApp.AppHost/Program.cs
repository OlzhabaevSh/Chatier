using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var chatierClusteringRedis = builder.AddRedis("chatierClusteringRedis");

// storages
var grainStorageRedis = builder.AddRedis("chatierGrainStorageRedis");
var systemRedis = builder.AddRedis("chatierSystemRedis");

// create orleans
var chatierOrleans = builder.AddOrleans("default")
    // clustering
    .WithClustering(chatierClusteringRedis)
    // grain storages
    .WithGrainStorage("chatStore", grainStorageRedis)
    .WithGrainStorage("notificationStore", grainStorageRedis)
    .WithGrainStorage("userStore", grainStorageRedis)
    .WithGrainStorage("userNotifications", grainStorageRedis)
    // system storages
    .WithGrainStorage("PubSubStore", systemRedis)
    .WithGrainStorage("UserGroupState", systemRedis)
    // streams
    .WithMemoryStreaming("MemoryStreamProvider")
    // reminders
    .WithMemoryReminders();

// configure silo
var siloApp = builder.AddProject<Chatier_Apps_SiloApp>("silo")
    .WithReference(chatierOrleans)
    .WithExternalHttpEndpoints()
    .WithReplicas(2);

// configure client
_ = builder.AddProject<Chatier_Apps_SignalrService>("backend")
    .WithReference(chatierOrleans.AsClient())
    .WithExternalHttpEndpoints()
    .WithReplicas(1)
    .WaitFor(siloApp);

// Build and run the application
using var app = builder.Build(); 
await app.RunAsync();