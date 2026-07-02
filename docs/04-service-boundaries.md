# Service Boundaries

SignalEye keeps protocol ingestion and gateway processing separate so each service has a narrow responsibility.

## Service Responsibilities

| Service | Responsibilities |
|---|---|
| `mqtt-protocol-service` | Connect to the MQTT broker, subscribe to telemetry topics, receive raw payloads, validate topic structure, extract `tenantId`, `siteId`, and `deviceId`, create `RawMqttMessage` records, forward raw messages to the gateway transport abstraction, and write operational logs. |
| `device-gateway-service` | Receive `RawMqttMessage` records, validate required fields, identify device and protocol type, create `CanonicalDeviceEvent` records, map supported M2000 input-register telemetry, preserve raw payloads, and write received, processed, and error logs. |

## Shared Components

| Component | Purpose |
|---|---|
| `SignalEye.Contracts` | Shared DTOs and message contracts. |
| `SignalEye.Telemetry` | Telemetry normalization and validation helpers. |
| `SignalEye.Modbus` | Isolated M2000 input-register mapping and parsing. |
| `SignalEye.Infrastructure` | Shared logging, messaging, and hosting helpers. |

## Non-Responsibilities

`mqtt-protocol-service` must not:

- Parse M2000 register meaning beyond protocol normalization.
- Write gateway processed telemetry logs.
- Store telemetry in a database.
- Send commands or remote configuration to devices.
- Trigger alerts or notifications.

`device-gateway-service` must not:

- Subscribe directly to MQTT topics.
- Manage MQTT broker sessions.
- Store telemetry in a database.
- Expose dashboard or API endpoints.
- Send commands or remote configuration to devices.
- Assume holding registers are available for M2000 devices.
