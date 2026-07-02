using SignalEye.Contracts;

namespace SignalEye.Infrastructure;

public interface IRawMqttMessagePublisher
{
    ValueTask PublishAsync(RawMqttMessage message, CancellationToken cancellationToken);
}

public interface IRawMqttMessageConsumer
{
    IAsyncEnumerable<RawMqttMessage> ConsumeAsync(CancellationToken cancellationToken);
}
