# Configuration

Configuration should be explicit, environment-friendly, and free of secrets. The values below are samples only.

## MQTT Broker

| Key | Sample value | Description |
|---|---|---|
| `Mqtt:Host` | `localhost` | MQTT broker host. |
| `Mqtt:Port` | `1883` | MQTT broker port. |
| `Mqtt:ClientId` | `signaleyes-mqtt-protocol-service` | MQTT client identifier. |
| `Mqtt:TelemetryTopic` | `signaleyes/+/+/+/telemetry` | Subscription filter for telemetry topics. |
| `Mqtt:Username` | `signaleyes-edge` | Optional MQTT username. |
| `Mqtt:Password` | omitted | Optional MQTT password; do not commit real values. |
| `Mqtt:TlsEnabled` | `false` | Enables TLS for broker connection when supported. |
| `Mqtt:QoS` | `1` | Requested MQTT subscription QoS. |
| `Mqtt:ReconnectBackoffSeconds` | `5` | Delay before reconnect attempts. |

## Message Transport

| Key | Sample value | Description |
|---|---|---|
| `MessageTransport:Type` | `Internal` | Internal transport implementation name. |
| `MessageTransport:Mode` | `InMemory` | First implementation mode for local/raw-message forwarding. |
| `RabbitMq:Host` | `localhost` | Future RabbitMQ host when RabbitMQ is introduced behind the abstraction. |
| `RabbitMq:RawMqttExchange` | `signaleyes.raw-mqtt` | Future exchange for raw MQTT messages if RabbitMQ is introduced. |
| `RabbitMq:TelemetryQueue` | `signaleyes.telemetry.device-gateway` | Future queue consumed by `device-gateway-service`. |

Use the internal message transport abstraction first with a local/in-memory placeholder. Do not make RabbitMQ required in the current phase.

## Logging

| Key | Sample value | Description |
|---|---|---|
| `TelemetryLogging:Directory` | `logs` | Root directory for JSON-lines telemetry logs. |
| `Service:Name` | `mqtt-protocol-service` | Service name included in operational logs. |

## Modbus Mapping

| Key | Sample value | Description |
|---|---|---|
| `Gateway:Modbus:MappingPath` | `config/modbus/edge-EN.csv` | Runtime CSV mapping source containing the active M2000 node subset to retrieve and map. |

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
    "TelemetryTopic": "signaleyes/+/+/+/telemetry",
    "Username": "signaleyes-edge",
    "Password": "",
    "TlsEnabled": false,
    "QoS": 1,
    "ReconnectBackoffSeconds": 5
  },
  "MessageTransport": {
    "Type": "Internal",
    "Mode": "InMemory"
  },
  "RabbitMq": {
    "Host": "localhost",
    "RawMqttExchange": "signaleyes.raw-mqtt",
    "TelemetryQueue": "signaleyes.telemetry.device-gateway"
  },
  "TelemetryLogging": {
    "Directory": "logs"
  },
  "Gateway": {
    "Modbus": {
      "MappingPath": "config/modbus/edge-EN.csv"
    }
  }
}
```

Do not commit secrets. Credentials, if needed later, should come from environment variables or a managed secret provider.
