# Configuration

Configuration should be explicit, environment-friendly, and free of secrets. The values below are samples only.

## MQTT Broker

| Key | Sample value | Description |
|---|---|---|
| `Mqtt:Host` | `localhost` | MQTT broker host. |
| `Mqtt:Port` | `1883` | MQTT broker port. |
| `Mqtt:ClientId` | `signaleyes-mqtt-protocol-service` | MQTT client identifier. |
| `Mqtt:TelemetryTopic` | `signaleyes/+/+/+/telemetry` | Subscription filter for telemetry topics. |

## Message Transport

| Key | Sample value | Description |
|---|---|---|
| `MessageTransport:Type` | `RabbitMq` | Internal transport implementation name. |
| `RabbitMq:Host` | `localhost` | RabbitMQ host when RabbitMQ is used. |
| `RabbitMq:TelemetryExchange` | `signaleyes.telemetry` | Exchange for telemetry envelopes. |
| `RabbitMq:TelemetryQueue` | `signaleyes.telemetry.device-gateway` | Queue consumed by `device-gateway-service`. |

If RabbitMQ is not used, keep the same service contract behind an internal message queue abstraction.

## Logging

| Key | Sample value | Description |
|---|---|---|
| `TelemetryLogging:Directory` | `logs` | Root directory for JSON-lines telemetry logs. |
| `Service:Name` | `mqtt-protocol-service` | Service name included in operational logs. |

## Sample Configuration

```json
{
  "Service": {
    "Name": "mqtt-protocol-service"
  },
  "Mqtt": {
    "Host": "localhost",
    "Port": 1883,
    "ClientId": "signaleyes-mqtt-protocol-service",
    "TelemetryTopic": "signaleyes/+/+/+/telemetry"
  },
  "MessageTransport": {
    "Type": "RabbitMq"
  },
  "RabbitMq": {
    "Host": "localhost",
    "TelemetryExchange": "signaleyes.telemetry",
    "TelemetryQueue": "signaleyes.telemetry.device-gateway"
  },
  "TelemetryLogging": {
    "Directory": "logs"
  }
}
```

Do not commit secrets. Credentials, if needed later, should come from environment variables or a managed secret provider.
