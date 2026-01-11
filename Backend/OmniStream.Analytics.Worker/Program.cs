using OmniStream.Analytics.Worker.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<OmniStreamSettings>()
       .Bind(builder.Configuration)
       .ValidateDataAnnotations()
       .ValidateOnStart();

builder.Services.AddSwaggerGen();

var host = builder.Build();
host.Run();
