# Tech Stack

SignalEyes uses a small worker-service stack for telemetry ingestion and processing.

| Layer | Technology | Usage |
|---|---|---|
| Runtime | .NET 10 | Service runtime and build target. |
| Language | C# | Application and shared library code. |
| Service model | Worker Services | Long-running MQTT and gateway workers. |
| Device transport | MQTT | Telemetry input from devices such as PUSR M100. |
| Internal transport | RabbitMQ or internal message queue abstraction | Forward normalized telemetry from `mqtt-protocol-service` to `device-gateway-service` when transport is enabled. |
| Message format | JSON | Payload and envelope serialization. |
| Log format | JSON-lines | Append-only operational and telemetry logs. |
| Deployment | Docker or systemd | Runtime deployment options when deployment assets are present. |

## Storage

No database is part of this phase. The only persistence target is JSON-lines log files under the configured log directory.

## Services

| Service | Type | Purpose |
|---|---|---|
| `mqtt-protocol-service` | .NET Worker Service | Receives MQTT telemetry and creates internal telemetry envelopes. |
| `device-gateway-service` | .NET Worker Service | Validates envelopes, parses supported telemetry, and writes logs. |
