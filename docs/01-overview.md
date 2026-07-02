# SignalEyes Overview

SignalEyes Device Gateway is a telemetry ingestion foundation for field devices that publish data through MQTT gateways. It receives raw device telemetry, normalizes it into an internal envelope, validates it in the gateway layer, parses supported M2000/Modbus input-register telemetry, and writes traceable JSON-lines log files.

This phase is telemetry ingestion only.

## Included Scope

| Area | Included in this phase |
|---|---|
| MQTT ingestion | Receive telemetry from MQTT devices such as PUSR M100. |
| Normalization | Convert raw MQTT payloads into an internal telemetry envelope. |
| Gateway processing | Validate envelopes and parse supported M2000 input-register telemetry. |
| Logging | Write received, processed, and error records to JSON-lines log files. |

## Excluded Scope

| Area | Status |
|---|---|
| Database persistence | Not included. |
| Dashboard or API | Not included. |
| Alerting or notifications | Not included. |
| Remote configuration | Not included. |
| Command sending | Not included. |
| Modbus holding-register writes | Not supported in this phase. |

## Phase Statement

The current implementation target is a clean, observable telemetry pipeline from MQTT input to local log files. It is not a full device-management platform yet.
