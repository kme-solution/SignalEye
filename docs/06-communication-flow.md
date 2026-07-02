# Communication Flow

SignalEye uses a one-way telemetry flow from MQTT input to local log files.

## Success Flow

1. An M2000 device exposes telemetry as Modbus input-register values.
2. A PUSR M100 or similar MQTT gateway publishes the telemetry payload to the MQTT broker.
3. `mqtt-protocol-service` receives a message on `signaleye/{tenantId}/{siteId}/{deviceId}/telemetry`.
4. `mqtt-protocol-service` validates the topic and extracts `tenantId`, `siteId`, and `deviceId`.
5. `mqtt-protocol-service` creates a `RawMqttMessage` that preserves the topic, metadata, payload encoding, and raw payload.
6. `mqtt-protocol-service` forwards the raw message to the Device Gateway Service through the RabbitMQ-backed transport abstraction.
7. `device-gateway-service` receives and validates the raw message.
8. `device-gateway-service` decodes JSON telemetry under `m`, looks up mapped nodes in `config/modbus/edge-EN.csv`, and creates a `CanonicalDeviceEvent`.
9. `device-gateway-service` writes received telemetry, processed telemetry, and operational records to JSON-lines log files.

## Failure Handling Flow

Invalid messages must be logged and skipped safely. A single bad topic, malformed payload, unsupported device type, or failed parse must not stop the service loop or block later messages.

| Failure | Expected handling |
|---|---|
| Invalid topic | Log the topic and reason, then skip the message. |
| Missing required raw-message field | Write a gateway error log entry, then skip processing. |
| Unsupported payload type | Preserve raw payload in received logs and skip protocol-specific parsing. |
| Malformed M2000 register payload | Write an error log entry with context and keep the service running. |
| Transport failure | Log operational error and retry according to the configured transport behavior. |

## Reliability Rule

Failures are isolated per message. Services should prefer structured logs, clear error reasons, and continued operation over process-wide failure for bad telemetry.
