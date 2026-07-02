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
| `MessageTransport:Type` | `RabbitMq` | Internal transport implementation name. |
| `RabbitMq:Host` | `localhost` | RabbitMQ host. |
| `RabbitMq:Port` | `5672` | RabbitMQ port. |
| `RabbitMq:Username` | `guest` | Optional RabbitMQ username. |
| `RabbitMq:Password` | omitted | Optional RabbitMQ password; do not commit real values. |
| `RabbitMq:RawMqttExchange` | `signaleyes.raw-mqtt` | Exchange used for raw MQTT messages. |
| `RabbitMq:RawMqttQueue` | `signaleyes.raw-mqtt.device-gateway` | Queue consumed by `device-gateway-service`. |
| `RabbitMq:RoutingKey` | `raw-mqtt` | Routing key bound between exchange and queue. |

Use RabbitMQ behind the internal message transport abstraction for service-to-service raw-message delivery.

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
    "Type": "RabbitMq"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "",
    "Password": "",
    "RawMqttExchange": "signaleyes.raw-mqtt",
    "RawMqttQueue": "signaleyes.raw-mqtt.device-gateway",
    "RoutingKey": "raw-mqtt"
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
