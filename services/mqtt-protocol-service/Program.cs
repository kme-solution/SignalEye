using SignalEyes.Infrastructure;
using SignalEyes.Telemetry;

var builder = Host.CreateApplicationBuilder(args);

var logDirectory = builder.Configuration.GetValue<string>("TelemetryLogging:Directory") ?? "logs";
var rabbitMqOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqTransportOptions>() ?? new RabbitMqTransportOptions();

builder.Services.Configure<MqttProtocolOptions>(builder.Configuration.GetSection("Mqtt"));
builder.Services.AddSingleton<RawMqttMessageFactory>();
builder.Services.AddSingleton(new TelemetryFileLogger(logDirectory));
builder.Services.AddSingleton(rabbitMqOptions);
builder.Services.AddSingleton<IRawMqttMessagePublisher, RabbitMqRawMqttMessagePublisher>();
builder.Services.AddHostedService<MqttProtocolWorker>();

var app = builder.Build();
await app.RunAsync();
