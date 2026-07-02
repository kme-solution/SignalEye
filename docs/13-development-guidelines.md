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
- Validate required envelope fields before protocol-specific parsing.

## Protocol Rules

- Keep protocol parsing isolated from worker orchestration code.
- Keep M2000 parsing limited to input registers.
- Do not assume M2000 holding registers are available.
- Do not implement Modbus writes or remote configuration in this phase.

## Testing Rules

- Add tests around telemetry envelope validation.
- Add tests around MQTT topic parsing.
- Add tests around M2000 input-register parsing.
- Add tests for malformed payloads and missing required fields.
- Use tests to protect per-message failure handling.
