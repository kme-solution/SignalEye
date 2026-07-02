# Naming Conventions

Consistent naming keeps service ownership and telemetry routing clear.

## Repository and Folders

| Item | Convention | Example |
|---|---|---|
| Repository | PascalCase product name | `SignalEye` |
| Worker service folders | kebab-case | `services/mqtt-protocol-service` |
| Shared library folders | PascalCase .NET project name | `src/SignalEye.Contracts` |
| Test folders | Match project or feature under test | `tests/SignalEye.Modbus.Tests` |
| Deployment folders | Lowercase by runtime target | `deploy/docker`, `deploy/systemd` |

## Services and Projects

| Type | Convention | Example |
|---|---|---|
| Service name | kebab-case | `mqtt-protocol-service` |
| Project file | Match service or library folder | `device-gateway-service.csproj` |
| C# namespace | PascalCase | `SignalEye.Modbus` |
| Class name | PascalCase, noun or noun phrase | `M2000InputRegisterParser` |
| Worker class | Service role plus `Worker` | `MqttProtocolWorker` |

## Configuration

| Area | Convention | Example |
|---|---|---|
| Section names | PascalCase | `Mqtt`, `RabbitMq`, `TelemetryLogging` |
| Key names | PascalCase | `Host`, `Port`, `ClientId`, `TelemetryTopic` |
| Service name value | kebab-case | `device-gateway-service` |

## Messages

| Item | Convention | Example |
|---|---|---|
| Raw MQTT message type | PascalCase C# type | `RawMqttMessage` |
| Canonical event type | PascalCase C# type | `CanonicalDeviceEvent` |
| JSON field names | camelCase when serialized for transport/logging | `deviceId`, `receivedAt`, `rawPayload` |
| Payload type value | kebab-case | `m2000-input-registers` |

## MQTT Topics

Telemetry topics use this format:

```text
signaleye/{tenantId}/{siteId}/{deviceId}/telemetry
```

Example:

```text
signaleye/acme/site-a/m2000-001/telemetry
```

| Segment | Meaning |
|---|---|
| `signaleye` | Product topic root. |
| `{tenantId}` | Tenant or customer identifier. |
| `{siteId}` | Site or facility identifier. |
| `{deviceId}` | Device identifier. |
| `telemetry` | Message category. |
