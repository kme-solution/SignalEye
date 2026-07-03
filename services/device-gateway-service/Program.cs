using SignalEye.Infrastructure;
using SignalEye.Modbus;
using SignalEye.Telemetry;

var builder = Host.CreateApplicationBuilder(args);

var logDirectory = builder.Configuration.GetValue<string>("TelemetryLogging:Directory") ?? "logs";
var maxLogSize = builder.Configuration.GetValue<long?>("TelemetryLogging:MaxDirectorySizeBytes") ?? JsonLineFileWriter.DefaultMaxDirectorySizeBytes;
var logRetentionDays = builder.Configuration.GetValue<int?>("TelemetryLogging:RetentionDays") ?? 7;
var mappingPath = builder.Configuration.GetValue<string>("Gateway:Modbus:MappingPath") ?? "config/modbus/edge-EN.csv";
var rabbitMqOptions = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqTransportOptions>() ?? new RabbitMqTransportOptions();

builder.Services.AddSingleton(new TelemetryFileLogger(logDirectory, maxLogSize, logRetentionDays));
builder.Services.AddSingleton<IModbusMappingProvider>(new CsvModbusMappingProvider(mappingPath));
builder.Services.AddSingleton<IModbusValueMapper, ModbusValueMapper>();
builder.Services.AddSingleton<PayloadDecoder>();
builder.Services.AddSingleton<CanonicalDeviceEventFactory>();
builder.Services.AddSingleton(rabbitMqOptions);
builder.Services.AddSingleton<IRawMqttMessageConsumer, RabbitMqRawMqttMessageConsumer>();
builder.Services.AddHostedService<DeviceGatewayWorker>();

var app = builder.Build();
await app.RunAsync();
