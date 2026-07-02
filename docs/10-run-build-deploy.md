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

## Package Source

The repository includes a source packaging script:

```bash
./scripts/package-source.sh
```

The script creates a timestamped archive under `_handover/` and excludes build output, logs, editor metadata, and git metadata.

## Docker Deployment Notes

`deploy/docker/` exists for Docker deployment assets. Keep container configuration aligned with the same service boundaries:

- One runtime process for `mqtt-protocol-service`.
- One runtime process for `device-gateway-service`.
- External MQTT broker and RabbitMQ configuration supplied through environment or mounted config.
- Log directory mounted as a persistent volume when logs must survive container replacement.

## systemd Deployment Notes

`deploy/systemd/` exists for systemd deployment assets. Each service should have a separate unit:

- `signaleye-mqtt-protocol-service.service`
- `signaleye-device-gateway-service.service`

Use systemd for process supervision, restart policy, and log forwarding. Application telemetry logs should still be written to the configured JSON-lines log directory.
