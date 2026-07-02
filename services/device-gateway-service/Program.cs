using SignalEyes.Infrastructure;
using SignalEyes.Modbus;

var builder = Host.CreateApplicationBuilder(args);

var logDirectory = builder.Configuration.GetValue<string>("TelemetryLogging:Directory") ?? "logs";

builder.Services.AddSingleton(new FileTelemetryLogger(logDirectory));
builder.Services.AddSingleton<M2000InputRegisterParser>();
builder.Services.AddHostedService<DeviceGatewayWorker>();

var app = builder.Build();
await app.RunAsync();
