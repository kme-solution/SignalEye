# Protocol Mapping

This document defines the telemetry topic, internal envelope, and supported M2000/Modbus mapping for the current phase.

## MQTT Topic Format

```text
signaleyes/{tenantId}/{siteId}/{deviceId}/telemetry
```

| Segment | Required | Description |
|---|---:|---|
| `signaleyes` | Yes | Fixed product root. |
| `{tenantId}` | Yes | Tenant or customer identifier. |
| `{siteId}` | Yes | Site or facility identifier. |
| `{deviceId}` | Yes | Device identifier. |
| `telemetry` | Yes | Fixed message category for telemetry ingestion. |

## Internal Telemetry Envelope

The envelope is the normalized message passed from `mqtt-protocol-service` to `device-gateway-service`.

| Field | Required | Description |
|---|---:|---|
| `tenantId` | Yes | Tenant parsed from the MQTT topic. |
| `siteId` | Yes | Site parsed from the MQTT topic. |
| `deviceId` | Yes | Device parsed from the MQTT topic. |
| `source` | Yes | Source transport, for example `mqtt`. |
| `receivedAt` | Yes | UTC timestamp when the service received or normalized the message. |
| `payloadType` | Yes | Payload classification, for example `raw` or `m2000-input-registers`. |
| `rawTopic` | Yes | Original MQTT topic. |
| `rawPayload` | Yes | Original payload exactly as received. |
| `protocol` | Optional | Protocol hint, for example `modbus`. |
| `deviceType` | Optional | Device type hint, for example `m2000`. |

Current code may use a smaller DTO while the transport is being built out. The table above is the target phase contract.

## M2000 Modbus Mapping Rules

M2000 supports input registers only. Holding registers are not available. Remote configuration through Modbus is not supported in this phase.

| Register index | Field | Scale | Notes |
|---:|---|---:|---|
| 0 | `gridVoltage` | 10 | Parsed as decimal voltage when present. |
| 1 | `batteryVoltage` | 10 | Parsed as decimal voltage when present. |
| 2 | `loadCurrent` | 10 | Parsed as decimal current when present. |
| 3 | `cabinetTemperature` | 10 | Parsed as decimal temperature when present. |

## Parsing Rules

- Treat the raw payload as the source of truth and preserve it in logs.
- Parse M2000 data only when the envelope identifies the payload as supported M2000 input-register telemetry.
- Missing optional register values should become null rather than failing the entire message.
- Malformed payloads should produce an error log entry and be skipped safely.
- Do not add holding-register reads, writes, remote command handling, or remote configuration in this phase.
