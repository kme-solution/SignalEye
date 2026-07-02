using SignalEyes.Telemetry;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<TelemetryNormalizer>();
builder.Services.AddHostedService<MqttProtocolWorker>();

var app = builder.Build();
await app.RunAsync();
