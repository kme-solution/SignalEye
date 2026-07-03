# Configuration

Configuration should be explicit, environment-friendly, and free of secrets. The values below are samples only.

## MQTT Server

| Key | Sample value | Description |
|---|---|---|
| `Mqtt:Port` | `1883` | TCP port exposed by the embedded MQTTnet server. |
| `Mqtt:Username` | `signaleye-device` | Optional device username. Blank allows anonymous connections. |
| `Mqtt:Password` | omitted | Device password; supply it through an environment variable. |

## Message Transport

| Key | Sample value | Description |
|---|---|---|
| `MessageTransport:Type` | `RabbitMq` | Internal transport implementation name. |
| `RabbitMq:Host` | `localhost` | RabbitMQ host. |
| `RabbitMq:Port` | `5672` | RabbitMQ port. |
| `RabbitMq:Username` | `guest` | Optional RabbitMQ username. |
| `RabbitMq:Password` | omitted | Optional RabbitMQ password; do not commit real values. |
| `RabbitMq:RawMqttExchange` | `signaleye.raw-mqtt` | Exchange used for raw MQTT messages. |
| `RabbitMq:RawMqttQueue` | `signaleye.raw-mqtt.device-gateway` | Queue consumed by `device-gateway-service`. |
| `RabbitMq:RoutingKey` | `raw-mqtt` | Routing key bound between exchange and queue. |

Use RabbitMQ behind the internal message transport abstraction for service-to-service raw-message delivery.

## Logging

| Key | Sample value | Description |
|---|---|---|
| `TelemetryLogging:Directory` | `logs` | Root directory for JSON-lines telemetry logs. |
| `TelemetryLogging:MaxDirectorySizeBytes` | `1073741824` | Maximum retained log size per service directory (1 GiB). |
| `TelemetryLogging:RetentionDays` | `7` | Maximum log age before deletion. |
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
    "Port": 1883,
    "Username": "signaleye-device",
    "Password": ""
  },
  "MessageTransport": {
    "Type": "RabbitMq"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "",
    "Password": "",
    "RawMqttExchange": "signaleye.raw-mqtt",
    "RawMqttQueue": "signaleye.raw-mqtt.device-gateway",
    "RoutingKey": "raw-mqtt"
  },
  "TelemetryLogging": {
    "Directory": "logs",
    "MaxDirectorySizeBytes": 1073741824,
    "RetentionDays": 7
  },
  "Gateway": {
    "Modbus": {
      "MappingPath": "config/modbus/edge-EN.csv"
    }
  }
}
```

Do not commit secrets. Credentials, if needed later, should come from environment variables or a managed secret provider.
