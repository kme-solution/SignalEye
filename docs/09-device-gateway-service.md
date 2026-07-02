# Device Gateway Service

`device-gateway-service` owns gateway-level telemetry validation, supported protocol parsing, and telemetry log output.

## Features

| Feature | Description |
|---|---|
| Receive telemetry envelope | Consume normalized telemetry from the configured internal transport. |
| Validate required fields | Require identifiers, source, received timestamp, payload type, and raw payload before processing. |
| Identify device/protocol type | Use envelope metadata and payload type to decide whether protocol-specific parsing applies. |
| Parse M2000 input-register telemetry | Parse supported M2000 input-register payloads when applicable. |
| Preserve raw payload | Keep the original payload available for traceability and troubleshooting. |
| Write received telemetry log | Append every valid received envelope to `gateway-received-yyyyMMdd.log`. |
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
