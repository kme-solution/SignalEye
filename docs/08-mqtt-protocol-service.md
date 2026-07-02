# MQTT Protocol Service

`mqtt-protocol-service` owns MQTT ingestion and protocol normalization. It is the boundary between MQTT devices and the internal SignalEye telemetry pipeline.

## Features

| Feature | Description |
|---|---|
| Host MQTT broker | Start an MQTTnet server on the configured TCP port and accept device connections. |
| Receive telemetry publish | Intercept publishes sent to `signaleye/{tenantId}/{siteId}/{deviceId}/telemetry`. |
| Receive raw payload | Read payload bytes/text from MQTT messages without losing original content. |
| Detect payload encoding | Store valid UTF-8 payloads as text and binary/non-UTF-8 payloads as base64. |
| Validate topic | Require the expected topic structure before forwarding telemetry. |
| Extract identifiers | Parse `tenantId`, `siteId`, and `deviceId` from the topic. |
| Create raw MQTT message | Build `RawMqttMessage` with broker metadata, topic, QoS, retained flag, receive timestamp, payload encoding, and payload. |
| Forward to device gateway | Publish the raw message through the RabbitMQ-backed internal transport abstraction. |
| Write operational logs | Record server startup, rejected connections, receive, forward, and validation errors. |

## Non-Responsibilities

`mqtt-protocol-service` must not:

- Parse M2000 register semantics.
- Write processed gateway telemetry logs.
- Store telemetry in a database.
- Expose a dashboard or API.
- Send commands or configuration updates to devices.
- Trigger alerts or notifications.

## Operational Notes

- Invalid topics should be logged and skipped safely.
- Raw payloads must be preserved in `RawMqttMessage`.
- MQTT server and transport failures should be observable through structured logs.
- The server supports an optional configured username and password. Add TLS and durable MQTT session persistence before exposing it to untrusted networks.
