# SignalEye Overview

SignalEye Device Gateway is a telemetry ingestion foundation for field devices that publish data through MQTT gateways. It receives raw MQTT telemetry, stores it as `RawMqttMessage`, normalizes it into `CanonicalDeviceEvent`, applies supported M2000/Modbus input-register mapping, and writes traceable JSON-lines log files.

This phase is telemetry ingestion only.

## Included Scope

| Area | Included in this phase |
|---|---|
| MQTT ingestion | Receive telemetry from MQTT devices such as PUSR M100. |
| Normalization | Convert `RawMqttMessage` records into `CanonicalDeviceEvent` records. |
| Gateway processing | Validate raw messages, map supported M2000 input-register telemetry, and preserve unknown metrics safely. |
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
