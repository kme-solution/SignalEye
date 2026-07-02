# Run, Build, and Deploy

This document covers local commands and lightweight deployment notes for the current telemetry-ingestion phase.

## Local Build

```bash
dotnet restore
dotnet build
```

## Local Test

```bash
dotnet test
```

## Local Run

Run each worker in a separate terminal:

```bash
dotnet run --project services/mqtt-protocol-service
```

```bash
dotnet run --project services/device-gateway-service
```

## Runtime Configuration Files

The Modbus mapping runtime source is expected at:

```text
config/modbus/edge-EN.csv
```

The repository includes `edge-EN.csv` as the active node subset. This file should contain only the M2000 nodes SignalEye retrieves.

## Docker Deployment Notes

The Docker Compose deployment includes both workers and RabbitMQ. `mqtt-protocol-service` hosts the MQTTnet server directly. From the repository root:

```bash
cp deploy/docker/.env.example deploy/docker/.env
```

Replace both password values in `.env` with long random values, then validate and start the stack:

```bash
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml config
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml up -d --build
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml ps
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml logs -f
```

Publish a local smoke-test message with any MQTT client. For example, if `mosquitto_pub` is installed as a client utility:

```bash
set -a; . deploy/docker/.env; set +a
mosquitto_pub -h 127.0.0.1 -p "$MQTT_PORT" \
  -u "$MQTT_USERNAME" -P "$MQTT_PASSWORD" \
  -t signaleye/demo/site-1/device-1/telemetry \
  -m '{"m":{"node1":230,"node2":12}}'
```

The telemetry log volume is shared by the workers. Inspect it with:

```bash
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml exec device-gateway-service \
  sh -c 'find /var/lib/signaleye/logs -type f -maxdepth 2 -print'
```

Stop the services without deleting persistent data:

```bash
docker compose --env-file deploy/docker/.env -f deploy/docker/compose.yaml down
```

Deployment boundaries:

- One runtime process for `mqtt-protocol-service`.
- One runtime process for `device-gateway-service`.
- Log directory mounted as a persistent volume when logs must survive container replacement.
- MQTT and RabbitMQ credentials supplied through `deploy/docker/.env`, which is ignored by Git.
- MQTTnet and RabbitMQ management ports bound to loopback by default.

MQTT TLS is not configured yet. Keep port 1883 on a trusted private network until TLS is implemented.

## systemd Deployment Notes

If systemd deployment assets are added, each service should have a separate unit:

- `signaleye-mqtt-protocol-service.service`
- `signaleye-device-gateway-service.service`

Use systemd for process supervision, restart policy, and log forwarding. Application telemetry logs should still be written to the configured JSON-lines log directory.
