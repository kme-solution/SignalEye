# SignalEyes

SignalEyes is a device telemetry ingestion foundation for field equipment connected through MQTT gateways. In the current phase, it focuses on receiving telemetry from MQTT devices such as PUSR M100, normalizing each raw MQTT payload into an internal telemetry envelope, forwarding that envelope to the Device Gateway Service, parsing supported M2000/Modbus input-register telemetry, and writing JSON-lines log files.

This phase is telemetry ingestion only. It does not include a database, dashboard, API, alerting, notification delivery, remote configuration, or command sending.

## Current Scope

Included:

- MQTT telemetry ingestion from devices such as PUSR M100.
- Internal telemetry envelope creation and forwarding.
- Device Gateway validation and supported M2000 input-register parsing.
- Raw, received, processed, and error log files.

Excluded:

- Database persistence.
- Dashboard or public API.
- Alerting and notification workflows.
- Remote device commands or configuration updates.
- Modbus holding-register writes.

## Documentation

| Document | Purpose |
|---|---|
| [01-overview.md](docs/01-overview.md) | Product and phase overview. |
| [02-tech-stack.md](docs/02-tech-stack.md) | Current technical stack. |
| [03-architecture.md](docs/03-architecture.md) | System architecture and component diagram. |
| [04-service-boundaries.md](docs/04-service-boundaries.md) | Responsibilities and non-responsibilities by service. |
| [05-naming-conventions.md](docs/05-naming-conventions.md) | Naming rules for code, config, messages, and topics. |
| [06-communication-flow.md](docs/06-communication-flow.md) | End-to-end telemetry flow and failure handling. |
| [07-protocol-mapping.md](docs/07-protocol-mapping.md) | MQTT topic, envelope, and M2000 mapping rules. |
| [08-mqtt-protocol-service.md](docs/08-mqtt-protocol-service.md) | MQTT Protocol Service contract. |
| [09-device-gateway-service.md](docs/09-device-gateway-service.md) | Device Gateway Service contract. |
| [10-run-build-deploy.md](docs/10-run-build-deploy.md) | Build, run, package, and deployment notes. |
| [11-configuration.md](docs/11-configuration.md) | Configuration keys and sample values. |
| [12-logging-standard.md](docs/12-logging-standard.md) | JSON-lines logging standard. |
| [13-development-guidelines.md](docs/13-development-guidelines.md) | Development rules for this phase. |

## Repository Structure

```text
SignalEyes/
  docs/                         Repository documentation foundation.
  services/
    mqtt-protocol-service/       Worker service for MQTT telemetry ingestion.
    device-gateway-service/      Worker service for gateway validation, parsing, and logs.
  src/
    SignalEyes.Contracts/        Shared telemetry DTOs and contracts.
    SignalEyes.Telemetry/        Telemetry normalization and validation helpers.
    SignalEyes.Modbus/           M2000 input-register parsing.
    SignalEyes.Infrastructure/   Shared logging, messaging, and hosting helpers.
  tests/                         Test projects for parsing, validation, and service behavior.
  deploy/
    docker/                      Docker deployment assets when present.
    systemd/                     systemd deployment assets when present.
  scripts/
    package-source.sh            Source packaging script.
```

## Build

```bash
dotnet restore
dotnet build
```
