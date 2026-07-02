using System.Text;
using System.Text.Json;
using SignalEyes.Contracts;
using RabbitMQ.Client;

namespace SignalEyes.Infrastructure;

public sealed class RabbitMqRawMqttMessagePublisher : IRawMqttMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMqTransportOptions _options;

    public RabbitMqRawMqttMessagePublisher(RabbitMqTransportOptions options)
    {
        _options = options;
        var factory = CreateFactory(options);
        _connection = factory.CreateConnection("signaleyes-mqtt-protocol-service");
        _channel = _connection.CreateModel();
        DeclareTopology();
    }

    public ValueTask PublishAsync(RawMqttMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = nameof(RawMqttMessage);
        properties.MessageId = message.MessageId;
        properties.Timestamp = new AmqpTimestamp(message.ReceivedAtUtc.ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: _options.RawMqttExchange,
            routingKey: _options.RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }

    private void DeclareTopology()
    {
        _channel.ExchangeDeclare(
            exchange: _options.RawMqttExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);
        _channel.QueueDeclare(
            queue: _options.RawMqttQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);
        _channel.QueueBind(
            queue: _options.RawMqttQueue,
            exchange: _options.RawMqttExchange,
            routingKey: _options.RoutingKey);
    }

    private static ConnectionFactory CreateFactory(RabbitMqTransportOptions options) =>
        new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = string.IsNullOrWhiteSpace(options.Username) ? ConnectionFactory.DefaultUser : options.Username,
            Password = string.IsNullOrWhiteSpace(options.Password) ? ConnectionFactory.DefaultPass : options.Password,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true
        };
}
