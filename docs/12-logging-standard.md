# Logging Standard

SignalEye uses JSON-lines logs for telemetry traceability and operational review. Each line is one complete JSON object.

## Log Folders

```text
logs/
  mqtt-protocol-service/
  device-gateway-service/
```

In Docker, this directory is `/var/lib/signaleye/logs` inside both worker containers and is bind-mounted to the repository's `logs/` directory by default. Override `TELEMETRY_LOG_PATH` in `deploy/docker/.env` to use another host directory. `HOST_UID` and `HOST_GID` run the workers as the host user so generated files remain directly accessible.

List all telemetry log files from the repository root:

```bash
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml \
  exec device-gateway-service \
  find /var/lib/signaleye/logs -maxdepth 2 -type f -print
```

Follow the current UTC day's processed telemetry:

```bash
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml \
  exec device-gateway-service \
  sh -c 'tail -F /var/lib/signaleye/logs/device-gateway-service/gateway-processed-$(date -u +%Y%m%d).log'
```

Follow raw MQTT telemetry:

```bash
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml \
  exec mqtt-protocol-service \
  sh -c 'tail -F /var/lib/signaleye/logs/mqtt-protocol-service/mqtt-telemetry-$(date -u +%Y%m%d).log'
```

The host directory survives container recreation and `docker compose down -v`.

## Rotation and Retention

- Logs roll daily using UTC dates.
- A file also rolls by size before its service directory would exceed 1 GiB. Additional files use `-001`, `-002`, and subsequent suffixes.
- Files older than seven days are deleted on the next write.
- If a service directory exceeds 1 GiB, its oldest inactive files are deleted on the next write.
- The age or size rule that is reached first determines retention.

Defaults can be changed with `TelemetryLogging:MaxDirectorySizeBytes` and `TelemetryLogging:RetentionDays`. Docker exposes them as `TELEMETRY_LOG_MAX_BYTES` and `TELEMETRY_LOG_RETENTION_DAYS` in `deploy/docker/.env`.

## Log Files

| Service | File | Purpose |
|---|---|---|
| `mqtt-protocol-service` | `mqtt-telemetry-yyyyMMdd.log` | MQTT receive and raw-message forwarding records. |
| `device-gateway-service` | `gateway-received-yyyyMMdd.log` | Valid raw messages received by the gateway. |
| `device-gateway-service` | `gateway-processed-yyyyMMdd.log` | Parsed and processed telemetry snapshots. |
| `device-gateway-service` | `gateway-error-yyyyMMdd.log` | Validation, parsing, and per-message failures. |

Size-rolled files append a sequence, for example `gateway-processed-20260703-001.log`.

## JSON-Lines Rules

- One JSON object per line.
- Use UTC timestamps in ISO 8601 format.
- Include `service`, `eventType`, and `timestamp` on every line.
- Include `tenantId`, `siteId`, and `gatewayId` when available.
- Store valid JSON in `payload` as a structured object and preserve the exact received text in `rawPayload` for traceability.
- Group processed readings under `devices` using the payload's top-level device key.
- Do not write secrets.

Only accepted MQTT publishes create telemetry files. Authentication failures and connection attempts are application diagnostics available through `docker compose logs mqtt-protocol-service`; they are not telemetry records.

## Sample Lines

MQTT telemetry:

```json
{"timestamp":"2026-07-02T00:00:00Z","service":"mqtt-protocol-service","eventType":"mqtt.telemetry.received","tenantId":"acme","siteId":"site-a","gatewayId":"m100-001","topic":"signaleye/acme/site-a/m100-001/telemetry","clientId":"123456","qos":1,"retained":false,"payloadEncoding":"utf-8","payload":{"device01":{"p":1,"s":1,"d":"m2000","fc":4,"m":{"node08":541}}},"rawPayload":"{\"device01\":{\"p\":1,\"s\":1,\"d\":\"m2000\",\"fc\":4,\"m\":{\"node08\":541}}}"}
```

Gateway received:

```json
{"timestamp":"2026-07-02T00:00:01Z","service":"device-gateway-service","eventType":"gateway.telemetry.received","tenantId":"acme","siteId":"site-a","gatewayId":"m100-001","source":"mqtt","topic":"signaleye/acme/site-a/m100-001/telemetry","clientId":"123456","payloadEncoding":"utf-8","payload":{"device01":{"p":1,"s":1,"d":"m2000","fc":4,"m":{"node08":541}}},"rawPayload":"{\"device01\":{\"p\":1,\"s\":1,\"d\":\"m2000\",\"fc\":4,\"m\":{\"node08\":541}}}"}
```

Gateway processed:

```json
{"timestamp":"2026-07-02T00:00:01Z","service":"device-gateway-service","eventType":"gateway.telemetry.processed","tenantId":"acme","siteId":"site-a","gatewayId":"m100-001","protocol":"modbus","devices":[{"deviceKey":"device01","deviceModel":"m2000","port":"1","slaveAddress":"1","functionCode":"4","readings":[{"name":"Rectifier Bus Voltage","value":"54.1","unit":"Volt","metadata":{"protocol":"modbus","registerAddress":"8","nodeName":"node08","dataType":"uint16"}}]}]}
```

Gateway error:

```json
{"timestamp":"2026-07-02T00:00:02Z","service":"device-gateway-service","eventType":"gateway.telemetry.error","tenantId":"acme","siteId":"site-a","deviceId":"m2000-001","errorCode":"invalid-register-payload","message":"M2000 input-register payload could not be parsed.","rawPayload":"not-register-data"}
```
