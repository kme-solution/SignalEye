# MQTT Protocol Service

`mqtt-protocol-service` owns MQTT ingestion and protocol normalization. It is the boundary between MQTT devices and the internal SignalEyes telemetry pipeline.

## Features

| Feature | Description |
|---|---|
| Connect to MQTT broker | Establish and maintain an MQTT client connection using configured broker settings. |
| Subscribe to telemetry topic | Subscribe to `signaleyes/{tenantId}/{siteId}/{deviceId}/telemetry`. |
| Receive raw payload | Read payload bytes/text from MQTT messages without losing original content. |
| Validate topic | Require the expected topic structure before forwarding telemetry. |
| Extract identifiers | Parse `tenantId`, `siteId`, and `deviceId` from the topic. |
| Create telemetry envelope | Build the internal envelope with source, timestamps, topic, identifiers, payload type, and raw payload. |
| Forward to device gateway | Publish or enqueue the envelope through the configured internal transport. |
| Write operational logs | Record connection, subscription, receive, forward, and validation errors. |

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
- Raw payloads must be preserved in the telemetry envelope.
- MQTT reconnect and transport retry behavior should be observable through structured logs.
