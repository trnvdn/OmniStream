using OmniStream.Analytics.Worker.Configuration;
using OmniStream.Analytics.Worker.Services;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<OmniStreamSettings>()
       .Bind(builder.Configuration)
       .ValidateDataAnnotations()
       .ValidateOnStart();

var redisConn = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConn));

builder.Services.AddSingleton<RedisMetricsRepository>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
