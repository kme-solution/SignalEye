using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using SignalEyes.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SignalEyes.Infrastructure;

public sealed class RabbitMqRawMqttMessageConsumer : IRawMqttMessageConsumer, IDisposable
{
    private readonly RabbitMqTransportOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    public RabbitMqRawMqttMessageConsumer(RabbitMqTransportOptions options)
    {
        _options = options;
        var factory = CreateFactory(options);
        _connection = factory.CreateConnection("signaleyes-device-gateway-service");
        _channel = _connection.CreateModel();
        DeclareTopology();
        _channel.BasicQos(0, 25, false);
    }

    public async IAsyncEnumerable<RawMqttMessage> ConsumeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var queue = Channel.CreateUnbounded<DeliveryItem>();
        var consumer = new AsyncEventingBasicConsumer(_channel);
        var consumerTag = string.Empty;

        consumer.Received += async (_, args) =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var message = JsonSerializer.Deserialize<RawMqttMessage>(Encoding.UTF8.GetString(args.Body.ToArray()));
            if (message is null)
            {
                _channel.BasicReject(args.DeliveryTag, false);
                return;
            }

            await queue.Writer.WriteAsync(new DeliveryItem(args.DeliveryTag, message), cancellationToken);
        };

        consumerTag = _channel.BasicConsume(
            queue: _options.RawMqttQueue,
            autoAck: false,
            consumer: consumer);

        try
        {
            await foreach (var item in queue.Reader.ReadAllAsync(cancellationToken))
            {
                yield return item.Message;
                if (_channel.IsOpen)
                {
                    _channel.BasicAck(item.DeliveryTag, false);
                }
            }
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(consumerTag) && _channel.IsOpen)
            {
                _channel.BasicCancel(consumerTag);
            }
        }
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

    private sealed record DeliveryItem(ulong DeliveryTag, RawMqttMessage Message);
}
