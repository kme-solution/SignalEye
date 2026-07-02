# Device Gateway Service

`device-gateway-service` owns gateway-level telemetry validation, supported protocol parsing, and telemetry log output.

## Features

| Feature | Description |
|---|---|
| Receive raw MQTT message | Consume `RawMqttMessage` records from the internal transport abstraction. |
| Validate required fields | Require identifiers, source, received timestamp, payload type, and raw payload before processing. |
| Identify device/protocol type | Use raw message metadata, topic context, and payload type to decide whether protocol-specific parsing applies. |
| Parse M2000 input-register telemetry | Parse supported M2000 input-register payloads using the configured CSV mapping when applicable. |
| Create canonical event | Convert valid raw messages into `CanonicalDeviceEvent` records. |
| Map object metrics | Resolve JSON metrics under `m`, such as `node08`, through the combined Modbus mapping source before creating canonical readings. |
| Preserve raw payload | Keep the original payload available for traceability and troubleshooting. |
| Write received telemetry log | Append every valid received raw message to `gateway-received-yyyyMMdd.log`. |
| Write processed telemetry log | Append parsed telemetry snapshots to `gateway-processed-yyyyMMdd.log`. |
| Write error log | Append validation and parsing failures to `gateway-error-yyyyMMdd.log`. |

## Non-Responsibilities

`device-gateway-service` must not:

- Subscribe directly to MQTT topics.
- Manage MQTT broker connection state.
- Store telemetry in a database.
- Expose dashboard or API endpoints.
- Send commands or remote configuration to devices.
- Implement alerting or notification delivery.
- Read or write M2000 holding registers.

## Processing Notes

- Fail safely per message and keep the service running.
- Preserve enough context in error logs to diagnose the failed message.
- Keep protocol parsing isolated in protocol-specific components.
- Load M2000 register metadata from `Gateway:Modbus:MappingPath`, currently `config/modbus/edge-EN.csv`.
- Unknown nodes should fall back to plain telemetry readings instead of being dropped.
- Current POC behavior does not apply Modbus mapping yet; this is the primary gateway normalization gap to close.
