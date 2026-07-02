# Logging Standard

SignalEyes uses JSON-lines logs for telemetry traceability and operational review. Each line is one complete JSON object.

## Log Folders

```text
logs/
  mqtt-protocol-service/
  device-gateway-service/
```

## Log Files

| Service | File | Purpose |
|---|---|---|
| `mqtt-protocol-service` | `mqtt-telemetry-yyyyMMdd.log` | MQTT receive and envelope forwarding records. |
| `device-gateway-service` | `gateway-received-yyyyMMdd.log` | Valid envelopes received by the gateway. |
| `device-gateway-service` | `gateway-processed-yyyyMMdd.log` | Parsed and processed telemetry snapshots. |
| `device-gateway-service` | `gateway-error-yyyyMMdd.log` | Validation, parsing, and per-message failures. |

## JSON-Lines Rules

- One JSON object per line.
- Use UTC timestamps in ISO 8601 format.
- Include `service`, `eventType`, and `timestamp` on every line.
- Include `tenantId`, `siteId`, and `deviceId` when available.
- Preserve `rawPayload` for telemetry traceability.
- Do not write secrets.

## Sample Lines

MQTT telemetry:

```json
{"timestamp":"2026-07-02T00:00:00Z","service":"mqtt-protocol-service","eventType":"mqtt.telemetry.received","tenantId":"acme","siteId":"site-a","deviceId":"m2000-001","topic":"signaleyes/acme/site-a/m2000-001/telemetry","payloadType":"m2000-input-registers","rawPayload":"2301,521,125,330"}
```

Gateway received:

```json
{"timestamp":"2026-07-02T00:00:01Z","service":"device-gateway-service","eventType":"gateway.telemetry.received","tenantId":"acme","siteId":"site-a","deviceId":"m2000-001","source":"mqtt","payloadType":"m2000-input-registers","rawPayload":"2301,521,125,330"}
```

Gateway processed:

```json
{"timestamp":"2026-07-02T00:00:01Z","service":"device-gateway-service","eventType":"gateway.telemetry.processed","tenantId":"acme","siteId":"site-a","deviceId":"m2000-001","deviceType":"m2000","gridVoltage":230.1,"batteryVoltage":52.1,"loadCurrent":12.5,"cabinetTemperature":33.0}
```

Gateway error:

```json
{"timestamp":"2026-07-02T00:00:02Z","service":"device-gateway-service","eventType":"gateway.telemetry.error","tenantId":"acme","siteId":"site-a","deviceId":"m2000-001","errorCode":"invalid-register-payload","message":"M2000 input-register payload could not be parsed.","rawPayload":"not-register-data"}
```
