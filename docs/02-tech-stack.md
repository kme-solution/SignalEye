# Tech Stack

SignalEye uses a small worker-service stack for telemetry ingestion and processing.

| Layer | Technology | Usage |
|---|---|---|
| Runtime | .NET 10 | Service runtime and build target. |
| Language | C# | Application and shared library code. |
| Service model | Worker Services | Long-running MQTT and gateway workers. |
| Device transport | MQTT | Telemetry input from devices such as PUSR M100. |
| MQTT client | MQTTnet | Broker connection, subscription, QoS, TLS, and message receive handling in `mqtt-protocol-service`. |
| Internal transport | RabbitMQ behind an internal message queue abstraction | Forward `RawMqttMessage` records from `mqtt-protocol-service` to `device-gateway-service`. |
| Message format | JSON | MQTT payloads, raw messages, canonical events, and log serialization. |
| Log format | JSON-lines | Append-only operational and telemetry logs. |
| Deployment | Docker or systemd | Runtime deployment options when deployment assets are present. |

## Storage

No database is part of this phase. The only persistence target is JSON-lines log files under the configured log directory.

## Services

| Service | Type | Purpose |
|---|---|---|
| `mqtt-protocol-service` | .NET Worker Service | Receives MQTT telemetry and creates `RawMqttMessage` records. |
| `device-gateway-service` | .NET Worker Service | Converts raw messages into `CanonicalDeviceEvent` records, maps supported telemetry, and writes logs. |
