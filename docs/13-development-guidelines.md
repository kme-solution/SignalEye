# Development Guidelines

These rules keep the current phase focused and implementation-ready.

## Scope Rules

- Keep services small and focused on one responsibility.
- Do not add database code in this phase.
- Do not add dashboard or API code in this phase.
- Do not add alerting or notification workflows in this phase.
- Do not add remote command or remote configuration features in this phase.

## Telemetry Rules

- Keep the raw payload for traceability.
- Fail safely per message and keep service loops running.
- Prefer structured logs over plain-text logs.
- Use JSON-lines for telemetry log files.
- Validate required raw-message fields before protocol-specific parsing.

## Protocol Rules

- Keep protocol parsing isolated from worker orchestration code.
- Keep M2000 parsing limited to input registers.
- Load M2000 register metadata from the configured CSV mapping file.
- Keep formula parsing separate from formula application until scaling is intentionally implemented.
- Use proof-of-concept code only as a behavior reference; rewrite implementation using SignalEyes standards.
- Keep Modbus TCP polling, raw MQTT message creation, and gateway processing as separate concerns.
- Do not assume M2000 holding registers are available.
- Do not implement Modbus writes or remote configuration in this phase.

## Testing Rules

- Add tests around `RawMqttMessage` validation and `CanonicalDeviceEvent` creation.
- Add tests around MQTT topic parsing.
- Add tests around M2000 input-register parsing.
- Add tests around CSV mapping loading and node-name resolution.
- Add tests around Modbus data type decoding, especially signed 16-bit and 32-bit high/low word values.
- Add tests around read-range grouping and max-register limits if SignalEyes implements direct Modbus TCP polling.
- Add tests for malformed payloads and missing required fields.
- Use tests to protect per-message failure handling.
