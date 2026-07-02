# Protocol Mapping

This document defines the telemetry topic, raw/canonical message contracts, and supported M2000/Modbus mapping for the current phase.

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

Legacy proof-of-concept services used `devices/{deviceId}/telemetry` as the default topic filter. New SignalEyes implementation uses only the tenant/site-aware topic above.

## Service Message Contracts

The primary service contract is `RawMqttMessage -> CanonicalDeviceEvent`. The MQTT Protocol Service creates a raw MQTT message, and the Device Gateway Service creates the canonical event.

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

Current code may use smaller DTOs while the transport is being built out. The table above describes the identifiers and payload fields that must be preserved across the raw and canonical contracts.

## Raw MQTT Envelope Reference

The proof-of-concept MQTT adapter converted every MQTT message into a raw message before gateway normalization. SignalEyes should preserve that behavior while using current project names and contracts.

Reference `RawMqttMessage` fields:

| Field | Description |
|---|---|
| `messageId` | Unique raw-message identifier. |
| `brokerHost` | MQTT broker host that supplied the message. |
| `brokerPort` | MQTT broker port. |
| `clientId` | MQTT client ID used by the adapter. |
| `topic` | Original MQTT topic. |
| `qos` | MQTT QoS level. |
| `retained` | MQTT retained flag. |
| `receivedAtUtc` | UTC receive timestamp. |
| `payloadEncoding` | `utf-8` for valid text payloads, `base64` for binary or non-UTF-8 payloads. |
| `payload` | UTF-8 payload text or base64-encoded binary payload. |
| `metadata` | Transport metadata such as client ID, broker host, broker port, and TLS flag. |

The POC published these raw messages to RabbitMQ exchange `signaleyes.raw-mqtt`. The current SignalEyes implementation should first use the internal transport abstraction with a local/in-memory placeholder. RabbitMQ can be introduced later behind the same abstraction if needed.

## M2000 Modbus Mapping Rules

M2000 supports input registers only. Holding registers are not available. Remote configuration through Modbus is not supported in this phase.

The Modbus decoder uses one runtime mapping source configured at `Gateway:Modbus:MappingPath`. This CSV is the active retrieval subset: it contains only the M2000 nodes SignalEyes should read and map, not every register from the M2000 reference table.

Default:

```json
{
  "Gateway": {
    "Modbus": {
      "MappingPath": "config/modbus/edge-EN.csv"
    }
  }
}
```

## Required CSV Columns

| Column | Purpose |
|---|---|
| `Device-name` | Source device profile name, for example `M2000`. |
| `Port-number` | M100 port used for polling. |
| `Slave-address` | Modbus slave address, default `1` for the current M2000 setup. |
| `Polling-interval` | Polling interval configured on the M100. |
| `Merge-collection` | M100 collection merge setting. Current mapping uses `OFF`. |
| `Reference` | Troubleshooting reference value. |
| `Node-name` | Telemetry node key, for example `node00`. |
| `Function-code` | Modbus function code, usually `4` for input registers. |
| `Register-Address` | Modbus register address. |
| `Data-type` | Data type hint, such as `uint16` or `uint32(ABCD)`. |
| `Response-Timeout` | M100 Modbus response timeout in milliseconds. |
| `Reporting-on-change` | M100 reporting-on-change setting. Current mapping uses `OFF`. |
| `Variation-range` | Change threshold when reporting-on-change is enabled. |
| `Mapped-address` | M100 mapped address field. The CSV export may contain this column more than once. |
| `Formula` | Optional scaling formula. Currently read but not applied. |

## M100 Polling Configuration

The current PUSR M100 profile reads the M2000 using Modbus TCP and publishes mapped values as object metrics. The CSV export uses these common values:

| Setting | Value |
|---|---|
| `Device-name` | `M2000` |
| `Port-number` | `1` |
| `Slave-address` | `1` |
| `Polling-interval` | `100` |
| `Merge-collection` | `OFF` |
| `Function-code` | `4` |
| `Response-Timeout` | `2000` |
| `Reporting-on-change` | `OFF` |
| `Mapped-address` | `N/A` |

Configured M100 node mappings:

| Reference | Node name | Function code | Register address | Data type |
|---:|---|---:|---:|---|
| 0 | `node00` | 4 | 0 | `uint16` |
| 1 | `node01` | 4 | 1 | `uint16` |
| 2 | `node02` | 4 | 2 | `uint16` |
| 3 | `node03` | 4 | 3 | `uint16` |
| 4 | `node04` | 4 | 4 | `uint16` |
| 5 | `node05` | 4 | 5 | `uint16` |
| 6 | `node06` | 4 | 6 | `uint16` |
| 7 | `node07` | 4 | 7 | `uint16` |
| 8 | `node08` | 4 | 8 | `uint16` |
| 9 | `node09` | 4 | 9 | `int16` |
| 10 | `node10` | 4 | 10 | `uint16` |
| 11 | `node11` | 4 | 11 | `int16` |
| 12 | `node12` | 4 | 12 | `uint16` |
| 13 | `node13` | 4 | 13 | `int16` |
| 14 | `node14` | 4 | 14 | `uint16` |
| 15 | `node15` | 4 | 15 | `uint16` |
| 16 | `node16` | 4 | 16 | `int16` |
| 17 | `node17` | 4 | 17 | `uint16` |
| 18 | `node18` | 4 | 18 | `uint16` |
| 19 | `node19` | 4 | 19 | `uint16` |
| 20 | `node20` | 4 | 20 | `uint16` |
| 21 | `node21` | 4 | 21 | `uint16` |
| 81 | `node81` | 4 | 81 | `uint32(ABCD)` |
| 83 | `node83` | 4 | 83 | `uint32(ABCD)` |
| 85 | `node85` | 4 | 85 | `uint32(ABCD)` |
| 87 | `node87` | 4 | 87 | `uint32(ABCD)` |
| 89 | `node89` | 4 | 89 | `uint32(ABCD)` |
| 91 | `node91` | 4 | 91 | `uint32(ABCD)` |
| 93 | `node93` | 4 | 93 | `uint32(ABCD)` |
| 95 | `node95` | 4 | 95 | `uint32(ABCD)` |
| 97 | `node97` | 4 | 97 | `uint32(ABCD)` |
| 99 | `node99` | 4 | 99 | `uint32(ABCD)` |
| 101 | `node101` | 4 | 101 | `uint32(ABCD)` |
| 102 | `node102` | 4 | 102 | `uint32(ABCD)` |

This configured profile intentionally polls a subset of the full Exicom M2000 input-register map. The decoder should use `config/modbus/edge-EN.csv` as the authoritative active node list. The full register map below is reference material only; do not treat it as the list of everything SignalEyes must retrieve.

## Reference Poller Behavior

A previous proof-of-concept Modbus TCP poller exists as a behavior reference only. Do not copy that code into SignalEyes. Reimplement the behavior using SignalEyes naming, service boundaries, DTOs, structured logs, and tests.

Reference behavior to preserve:

| Area | Expected behavior |
|---|---|
| Enablement | Polling is controlled by configuration and should not start when disabled. |
| Host validation | If polling is enabled but the host is empty, log a warning and do not start polling. |
| Node selection | Poll only mapping rows with Function 4 input-register nodes. |
| Node ordering | Order nodes by register address, then node name. |
| Read ranges | Build compact read ranges from mapped nodes instead of reading each register separately. |
| Register limit | Keep each Modbus read within the protocol limit of 125 registers. |
| Register gaps | Merge nearby ranges only when the configured maximum gap allows it. |
| Data types | Decode `uint16`, `int16`/`sint16`, `uint8`, `uint32(ABCD)`, and `int32`/`sint32` variants. |
| 32-bit values | Read the high word at `Register-Address` and the low word at `Register-Address + 1`. |
| Formulas | Formula support may parse multiply/divide expressions, but production implementation should keep formula handling isolated and tested. |
| Timeouts | Apply per-poll and per-read timeouts from configuration. |
| Failures | Treat poll failures as operational errors, log context, and continue future polling. |

Reference behavior to avoid:

- Do not put low-level Modbus TCP polling directly into unrelated service orchestration code.
- Do not expose proof-of-concept monitor bridge types in SignalEyes contracts.
- Do not return decoded values as loosely typed strings where typed telemetry records are required.
- Do not let one failed node or range stop the worker permanently.

## Node Naming

Current node format maps directly to the register address:

```text
node00 -> register 0
node01 -> register 1
node81 -> register 81
```

The decoder also accepts legacy POC names and maps them by ordinal position:

```text
node0101 -> register 0
node0109 -> register 8
```

## Object Metrics

When telemetry uses object form, the decoder looks up each node key in the CSV, attaches reference/register metadata, and exposes the value for downstream processing.

```json
{
  "m": {
    "node08": 541
  }
}
```

For the example above, the decoder resolves `node08` against the configured mapping file and preserves the raw metric value with its mapping metadata.

## Modbus-Aware Canonical Reading

Current proof-of-concept gateway behavior treats metric names as plain reading names. For example:

```json
{
  "name": "node08",
  "value": "541",
  "unit": null
}
```

SignalEyes should enrich mapped Modbus nodes before creating canonical readings. Expected mapped output:

```json
{
  "name": "Rectifier Bus Voltage",
  "value": "54.1",
  "unit": "Volt",
  "metadata": {
    "protocol": "modbus",
    "functionCode": "4",
    "registerAddress": "8",
    "nodeName": "node08",
    "dataType": "uint16"
  }
}
```

Unknown or unmapped nodes should still be accepted as raw/plain telemetry readings, with enough metadata to troubleshoot why no mapping was applied.

## Gateway Mapping Flow

Add Modbus mapping inside `device-gateway-service` after JSON metric extraction and before canonical event creation:

```text
Raw MQTT message
  -> PayloadDecoder
  -> JSON metrics under "m"
  -> ModbusMappingService lookup by node name
  -> Apply data type, formula, unit, and register metadata
  -> CanonicalDeviceEvent readings
```

Suggested component boundaries:

```csharp
public sealed record ModbusRegisterMapping(
    string NodeName,
    int FunctionCode,
    int RegisterAddress,
    string DataType,
    string? Unit,
    string? DisplayName,
    string? Formula);

public interface IModbusMappingProvider
{
    bool TryGetByNodeName(string nodeName, out ModbusRegisterMapping mapping);
}

public interface IModbusValueMapper
{
    TelemetryReading Map(string nodeName, string rawValue, DateTimeOffset timestampUtc);
}
```

These are design references, not final API requirements. Implement them in the style that best fits the current SignalEyes codebase.

## Exicom M2000 Register Reference

The M2000 register reference is based on Exicom Hybrid Power Plants using Controller M2000, Document No. `TS_HE94007x Rev. 0`.

### Communication Configuration

| Setting | Value |
|---|---|
| Mode of transmission | Modbus RTU |
| Interface | RS-485, 2-wire |
| Baud rate | 19200 bps |
| Data bits | 8 |
| Parity | None |
| Stop bits | 1 |
| Supported function code | Function 4 (`04h`) - read input data registers |
| Device slave address range | 1-247, default `1` |

### Input Register Map

Only Function 4 input registers are in scope. The source table contains duplicate addresses at `122` and `219`; keep those rows visible in documentation and resolve implementation behavior through the configured CSV mapping.

| Reg. Address | Parameter | Data Type | Units | Remarks / Scaling |
|---:|---|---|---|---|
| 0 | Alarm Status Reg. 1 | BOOL |  | See Table 11 for details |
| 1 | Alarm Status Reg. 2 | BOOL |  | See Table 11 for details |
| 2 | Alarm Status Reg. 3 | BOOL |  | See Table 11 for details |
| 3 | Alarm Status Reg. 4 | BOOL |  | See Table 11 for details |
| 4 | Rectifier Alarm Status | BOOL |  | See Table 11 for details |
| 5 | Rectifier Communication Status | BOOL |  | See Table 11 for details |
| 6 | Solar Alarm Status | BOOL |  | See Table 11 for details |
| 7 | Solar Communication Status | BOOL |  | See Table 11 for details |
| 8 | Rectifier Bus Voltage | UINT16 | Volt | Divide by 10 to get value |
| 9 | System Current | SINT16 | Amp |  |
| 10 | Load Voltage | UINT16 | Volt | Divide by 10 to get value |
| 11 | Load Current | SINT16 | Amp | Divide by 10 to get value |
| 12 | Battery 1 Voltage | UINT16 | Volt | Divide by 10 to get value |
| 13 | Battery 1 Current | SINT16 | Amp | Divide by 10 to get value |
| 14 | Battery 1 Remaining AH % | UINT16 | % | Divide by 10 to get value |
| 15 | Battery 2 Voltage | UINT16 | Volt | Divide by 10 to get value |
| 16 | Battery 2 Current | SINT16 | Amp | Divide by 10 to get value |
| 17 | Battery 2 Remaining AH % | UINT16 | % | Divide by 10 to get value |
| 18 | Line 1 Voltage | UINT16 | Volt |  |
| 19 | Line 2 Voltage | UINT16 | Volt |  |
| 20 | Line 3 Voltage | UINT16 | Volt |  |
| 21 | Battery Temperature | UINT16 | C | Divide by 10 to get value |
| 22 | Reserved | UINT16 |  |  |
| 23 | Reserved | UINT16 |  |  |
| 24 | Output Voltage Rectifier [1] | UINT16 | Volt | Divide by 10 to get value |
| 25 | Output Voltage Rectifier [2] | UINT16 | Volt | Divide by 10 to get value |
| 26 | Output Voltage Rectifier [3] | UINT16 | Volt | Divide by 10 to get value |
| 27 | Output Voltage Rectifier [4] | UINT16 | Volt | Divide by 10 to get value |
| 28 | Output Voltage Rectifier [5] | UINT16 | Volt | Divide by 10 to get value |
| 29 | Output Voltage Rectifier [6] | UINT16 | Volt | Divide by 10 to get value |
| 30 | Output Voltage Rectifier [7] | UINT16 | Volt | Divide by 10 to get value |
| 31 | Output Voltage Rectifier [8] | UINT16 | Volt | Divide by 10 to get value |
| 32 | Output Voltage Rectifier [9] | UINT16 | Volt | Divide by 10 to get value |
| 33 | Output Voltage Rectifier [10] | UINT16 | Volt | Divide by 10 to get value |
| 34 | Output Voltage Rectifier [11] | UINT16 | Volt | Divide by 10 to get value |
| 35 | Output Voltage Rectifier [12] | UINT16 | Volt | Divide by 10 to get value |
| 36 | Output Voltage Rectifier [13] | UINT16 | Volt | Divide by 10 to get value |
| 37 | Output Voltage Rectifier [14] | UINT16 | Volt | Divide by 10 to get value |
| 38 | Output Voltage Rectifier [15] | UINT16 | Volt | Divide by 10 to get value |
| 39 | Output Voltage Rectifier [16] | UINT16 | Volt | Divide by 10 to get value |
| 40 | Output Current Rectifier [1] | UINT16 | Amp | Divide by 10 to get value |
| 41 | Output Current Rectifier [2] | UINT16 | Amp | Divide by 10 to get value |
| 42 | Output Current Rectifier [3] | UINT16 | Amp | Divide by 10 to get value |
| 43 | Output Current Rectifier [4] | UINT16 | Amp | Divide by 10 to get value |
| 44 | Output Current Rectifier [5] | UINT16 | Amp | Divide by 10 to get value |
| 45 | Output Current Rectifier [6] | UINT16 | Amp | Divide by 10 to get value |
| 46 | Output Current Rectifier [7] | UINT16 | Amp | Divide by 10 to get value |
| 47 | Output Current Rectifier [8] | UINT16 | Amp | Divide by 10 to get value |
| 48 | Output Current Rectifier [9] | UINT16 | Amp | Divide by 10 to get value |
| 49 | Output Current Rectifier [10] | UINT16 | Amp | Divide by 10 to get value |
| 50 | Output Current Rectifier [11] | UINT16 | Amp | Divide by 10 to get value |
| 51 | Output Current Rectifier [12] | UINT16 | Amp | Divide by 10 to get value |
| 52 | Output Current Rectifier [13] | UINT16 | Amp | Divide by 10 to get value |
| 53 | Output Current Rectifier [14] | UINT16 | Amp | Divide by 10 to get value |
| 54 | Output Current Rectifier [15] | UINT16 | Amp | Divide by 10 to get value |
| 55 | Output Current Rectifier [16] | UINT16 | Amp | Divide by 10 to get value |
| 56 | Rectifier Load Share Deviation % | UINT16 | % |  |
| 57 | Rectifier Current Sum (High Word) | UINT32 | Amp | Divide by 10 to get value |
| 58 | Rectifier Current Sum (Low Word) | UINT32 | Amp | Divide by 10 to get value |
| 59 | Active Rectifier Count | UINT8 |  |  |
| 60 | Reserved | UINT16 |  |  |
| 61 | Reserved | UINT16 |  |  |
| 62 | Reserved | UINT16 |  |  |
| 63 | Reserved | UINT16 |  |  |
| 64 | Reserved | UINT16 |  |  |
| 65 | Reserved | UINT16 |  |  |
| 66 | Reserved | UINT16 |  |  |
| 67 | Reserved | UINT16 |  |  |
| 68 | Reserved | UINT16 |  |  |
| 69 | Reserved | UINT16 |  |  |
| 70 | Reserved | UINT16 |  |  |
| 71 | Reserved | UINT16 |  |  |
| 72 | Reserved | UINT16 |  |  |
| 73 | Reserved | UINT16 |  |  |
| 74 | Reserved | UINT16 |  |  |
| 75 | Reserved | UINT16 |  |  |
| 76 | Reserved | UINT16 |  |  |
| 77 | Reserved | UINT16 |  |  |
| 78 | Reserved | UINT16 |  |  |
| 79 | Reserved | UINT16 |  |  |
| 80 | Reserved | UINT16 |  |  |
| 81 | Operator Power[1] (High Word) | UINT32 | Watt |  |
| 82 | Operator Power[1] (Low Word) | UINT32 | Watt |  |
| 83 | Operator Cumulative Power[1] (High Word) | UINT32 | Watt |  |
| 84 | Operator Cumulative Power[1] (Low Word) | UINT32 | Watt |  |
| 85 | Battery Charge Counter (High Word) | UINT32 |  |  |
| 86 | Battery Charge Counter (Low Word) | UINT32 |  |  |
| 87 | Battery Discharge Counter (High Word) | UINT32 |  |  |
| 88 | Battery Discharge Counter (Low Word) | UINT32 |  |  |
| 89 | Battery Cumulative Charge Time (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 90 | Battery Cumulative Charge Time (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 91 | Battery Cumulative Discharge Time (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 92 | Battery Cumulative Discharge Time (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 93 | Battery Cumulative Charge Power (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 94 | Battery Cumulative Charge Power (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 95 | Battery Cumulative Discharge Power (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 96 | Battery Cumulative Discharge Power (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 97 | Battery Cumulative Discharge AH (High Word) | UINT32 | KAH | Divide by 10 to get value |
| 98 | Battery Cumulative Discharge AH (Low Word) | UINT32 | KAH | Divide by 10 to get value |
| 99 | Total Solar Cumulative Time (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 100 | Total Solar Cumulative Time (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 101 | Total Solar Cumulative Power (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 102 | Total Solar Cumulative Power (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 103 | Solar Rectifier Input Voltage [1] | UINT16 | Volt | Divide by 10 to get value |
| 104 | Solar Rectifier Input Voltage [2] | UINT16 | Volt | Divide by 10 to get value |
| 105 | Solar Rectifier Input Voltage [3] | UINT16 | Volt | Divide by 10 to get value |
| 106 | Solar Rectifier Input Voltage [4] | UINT16 | Volt | Divide by 10 to get value |
| 107 | Solar Rectifier Input Voltage [5] | UINT16 | Volt | Divide by 10 to get value |
| 108 | Solar Rectifier Input Voltage [6] | UINT16 | Volt | Divide by 10 to get value |
| 109 | Solar Rectifier Input Voltage [7] | UINT16 | Volt | Divide by 10 to get value |
| 110 | Solar Rectifier Input Voltage [8] | UINT16 | Volt | Divide by 10 to get value |
| 111 | Solar Rectifier Input Voltage [9] | UINT16 | Volt | Divide by 10 to get value |
| 112 | Solar Rectifier Input Voltage [10] | UINT16 | Volt | Divide by 10 to get value |
| 113 | Solar Rectifier Input Current [1] | UINT16 | Amp | Divide by 10 to get value |
| 114 | Solar Rectifier Input Current [2] | UINT16 | Amp | Divide by 10 to get value |
| 115 | Solar Rectifier Input Current [3] | UINT16 | Amp | Divide by 10 to get value |
| 116 | Solar Rectifier Input Current [4] | UINT16 | Amp | Divide by 10 to get value |
| 117 | Solar Rectifier Input Current [5] | UINT16 | Amp | Divide by 10 to get value |
| 118 | Solar Rectifier Input Current [6] | UINT16 | Amp | Divide by 10 to get value |
| 119 | Solar Rectifier Input Current [7] | UINT16 | Amp | Divide by 10 to get value |
| 120 | Solar Rectifier Input Current [8] | UINT16 | Amp | Divide by 10 to get value |
| 121 | Solar Rectifier Input Current [9] | UINT16 | Amp | Divide by 10 to get value |
| 122 | Solar Rectifier Input Current [10] | UINT16 | Amp | Divide by 10 to get value |
| 122 | Solar Rectifier Input Power[1] (High Word) | UINT32 | Watt |  |
| 123 | Solar Rectifier Input Power[1] (Low Word) | UINT32 | Watt |  |
| 124 | Solar Rectifier Input Power[2] (High Word) | UINT32 | Watt |  |
| 125 | Solar Rectifier Input Power[2] (Low Word) | UINT32 | Watt |  |
| 126 | Solar Rectifier Input Power[3] (High Word) | UINT32 | Watt |  |
| 127 | Solar Rectifier Input Power[3] (Low Word) | UINT32 | Watt |  |
| 128 | Solar Rectifier Input Power[4] (High Word) | UINT32 | Watt |  |
| 129 | Solar Rectifier Input Power[4] (Low Word) | UINT32 | Watt |  |
| 130 | Solar Rectifier Input Power[5] (High Word) | UINT32 | Watt |  |
| 131 | Solar Rectifier Input Power[5] (Low Word) | UINT32 | Watt |  |
| 132 | Solar Rectifier Input Power[6] (High Word) | UINT32 | Watt |  |
| 133 | Solar Rectifier Input Power[6] (Low Word) | UINT32 | Watt |  |
| 134 | Solar Rectifier Input Power[7] (High Word) | UINT32 | Watt |  |
| 135 | Solar Rectifier Input Power[7] (Low Word) | UINT32 | Watt |  |
| 136 | Solar Rectifier Input Power[8] (High Word) | UINT32 | Watt |  |
| 137 | Solar Rectifier Input Power[8] (Low Word) | UINT32 | Watt |  |
| 138 | Solar Rectifier Input Power[9] (High Word) | UINT32 | Watt |  |
| 139 | Solar Rectifier Input Power[9] (Low Word) | UINT32 | Watt |  |
| 140 | Solar Rectifier Input Power[10] (High Word) | UINT32 | Watt |  |
| 141 | Solar Rectifier Input Power[10] (Low Word) | UINT32 | Watt |  |
| 142 | Solar Rectifier Total Power[1] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 143 | Solar Rectifier Total Power[1] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 144 | Solar Rectifier Total Power[2] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 145 | Solar Rectifier Total Power[2] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 146 | Solar Rectifier Total Power[3] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 147 | Solar Rectifier Total Power[3] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 148 | Solar Rectifier Total Power[4] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 149 | Solar Rectifier Total Power[4] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 150 | Solar Rectifier Total Power[5] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 151 | Solar Rectifier Total Power[5] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 152 | Solar Rectifier Total Power[6] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 153 | Solar Rectifier Total Power[6] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 154 | Solar Rectifier Total Power[7] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 155 | Solar Rectifier Total Power[7] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 156 | Solar Rectifier Total Power[8] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 157 | Solar Rectifier Total Power[8] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 158 | Solar Rectifier Total Power[9] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 159 | Solar Rectifier Total Power[9] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 160 | Solar Rectifier Total Power[10] (High Word) | UINT32 | KWH | Divide by 10 to get value |
| 161 | Solar Rectifier Total Power[10] (Low Word) | UINT32 | KWH | Divide by 10 to get value |
| 162 | Solar Rectifier UP Time[1] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 163 | Solar Rectifier UP Time[1] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 164 | Solar Rectifier UP Time[2] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 165 | Solar Rectifier UP Time[2] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 166 | Solar Rectifier UP Time[3] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 167 | Solar Rectifier UP Time[3] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 168 | Solar Rectifier UP Time[4] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 169 | Solar Rectifier UP Time[4] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 170 | Solar Rectifier UP Time[5] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 171 | Solar Rectifier UP Time[5] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 172 | Solar Rectifier UP Time[6] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 173 | Solar Rectifier UP Time[6] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 174 | Solar Rectifier UP Time[7] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 175 | Solar Rectifier UP Time[7] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 176 | Solar Rectifier UP Time[8] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 177 | Solar Rectifier UP Time[8] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 178 | Solar Rectifier UP Time[9] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 179 | Solar Rectifier UP Time[9] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 180 | Solar Rectifier UP Time[10] (High Word) | UINT32 | Hour | Divide by 10 to get value |
| 181 | Solar Rectifier UP Time[10] (Low Word) | UINT32 | Hour | Divide by 10 to get value |
| 183 | Output Voltage Solar Charger [1] | UINT16 | Volt | Divide by 10 to get value |
| 184 | Output Voltage Solar Charger [2] | UINT16 | Volt | Divide by 10 to get value |
| 185 | Output Voltage Solar Charger [3] | UINT16 | Volt | Divide by 10 to get value |
| 186 | Output Voltage Solar Charger [4] | UINT16 | Volt | Divide by 10 to get value |
| 187 | Output Voltage Solar Charger [5] | UINT16 | Volt | Divide by 10 to get value |
| 188 | Output Voltage Solar Charger [6] | UINT16 | Volt | Divide by 10 to get value |
| 189 | Output Voltage Solar Charger [7] | UINT16 | Volt | Divide by 10 to get value |
| 190 | Output Voltage Solar Charger [8] | UINT16 | Volt | Divide by 10 to get value |
| 191 | Output Voltage Solar Charger [9] | UINT16 | Volt | Divide by 10 to get value |
| 192 | Output Voltage Solar Charger [10] | UINT16 | Volt | Divide by 10 to get value |
| 193 | Output Current Solar Charger [1] | UINT16 | Amp | Divide by 10 to get value |
| 194 | Output Current Solar Charger [2] | UINT16 | Amp | Divide by 10 to get value |
| 195 | Output Current Solar Charger [3] | UINT16 | Amp | Divide by 10 to get value |
| 196 | Output Current Solar Charger [4] | UINT16 | Amp | Divide by 10 to get value |
| 197 | Output Current Solar Charger [5] | UINT16 | Amp | Divide by 10 to get value |
| 198 | Output Current Solar Charger [6] | UINT16 | Amp | Divide by 10 to get value |
| 199 | Output Current Solar Charger [7] | UINT16 | Amp | Divide by 10 to get value |
| 200 | Output Current Solar Charger [8] | UINT16 | Amp | Divide by 10 to get value |
| 201 | Output Current Solar Charger [9] | UINT16 | Amp | Divide by 10 to get value |
| 202 | Output Current Solar Charger [10] | UINT16 | Amp | Divide by 10 to get value |
| 203 | Total Solar Charger Current | UINT16 | Amp | Divide by 10 to get value |
| 204 | Reserved | UINT16 |  |  |
| 205 | Reserved | UINT16 |  |  |
| 206 | Reserved | UINT16 |  |  |
| 207 | Reserved | UINT16 |  |  |
| 208 | Reserved | UINT16 |  |  |
| 209 | Reserved | UINT16 |  |  |
| 210 | Reserved | UINT16 |  |  |
| 211 | Reserved | UINT16 |  |  |
| 212 | Reserved | UINT16 |  |  |
| 213 | Reserved | UINT16 |  |  |
| 214 | Reserved | UINT16 |  |  |
| 215 | Reserved | UINT16 |  |  |
| 216 | Reserved | UINT16 |  |  |
| 217 | Reserved | UINT16 |  |  |
| 218 | Reserved | UINT16 |  |  |
| 219 | Reserved | UINT16 |  |  |
| 219 | Solar Charger Version Revision [1] (High Word) | UINT32 |  |  |
| 220 | Solar Charger Version Revision [1] (Low Word) | UINT32 |  |  |
| 221 | Solar Charger Version Revision [2] (High Word) | UINT32 |  |  |
| 222 | Solar Charger Version Revision [2] (Low Word) | UINT32 |  |  |
| 223 | Solar Charger Version Revision [3] (High Word) | UINT32 |  |  |
| 224 | Solar Charger Version Revision [3] (Low Word) | UINT32 |  |  |
| 225 | Solar Charger Version Revision [4] (High Word) | UINT32 |  |  |
| 226 | Solar Charger Version Revision [4] (Low Word) | UINT32 |  |  |
| 227 | Solar Charger Version Revision [5] (High Word) | UINT32 |  |  |
| 228 | Solar Charger Version Revision [5] (Low Word) | UINT32 |  |  |
| 229 | Solar Charger Version Revision [6] (High Word) | UINT32 |  |  |
| 230 | Solar Charger Version Revision [6] (Low Word) | UINT32 |  |  |
| 231 | Solar Charger Version Revision [7] (High Word) | UINT32 |  |  |
| 232 | Solar Charger Version Revision [7] (Low Word) | UINT32 |  |  |
| 233 | Solar Charger Version Revision [8] (High Word) | UINT32 |  |  |
| 234 | Solar Charger Version Revision [8] (Low Word) | UINT32 |  |  |
| 235 | Solar Charger Version Revision [9] (High Word) | UINT32 |  |  |
| 236 | Solar Charger Version Revision [9] (Low Word) | UINT32 |  |  |
| 237 | Solar Charger Version Revision [10] (High Word) | UINT32 |  |  |
| 238 | Solar Charger Version Revision [10] (Low Word) | UINT32 |  |  |
| 239 | Rectifier Version Revision [1] (High Word) | UINT32 |  |  |
| 240 | Rectifier Version Revision [1] (Low Word) | UINT32 |  |  |
| 241 | Rectifier Version Revision [2] (High Word) | UINT32 |  |  |
| 242 | Rectifier Version Revision [2] (Low Word) | UINT32 |  |  |
| 243 | Rectifier Version Revision [3] (High Word) | UINT32 |  |  |
| 244 | Rectifier Version Revision [3] (Low Word) | UINT32 |  |  |
| 245 | Rectifier Version Revision [4] (High Word) | UINT32 |  |  |
| 246 | Rectifier Version Revision [4] (Low Word) | UINT32 |  |  |
| 247 | Rectifier Version Revision [5] (High Word) | UINT32 |  |  |
| 248 | Rectifier Version Revision [5] (Low Word) | UINT32 |  |  |
| 249 | Rectifier Version Revision [6] (High Word) | UINT32 |  |  |
| 250 | Rectifier Version Revision [6] (Low Word) | UINT32 |  |  |
| 251 | Rectifier Version Revision [7] (High Word) | UINT32 |  |  |
| 252 | Rectifier Version Revision [7] (Low Word) | UINT32 |  |  |
| 253 | Rectifier Version Revision [8] (High Word) | UINT32 |  |  |
| 254 | Rectifier Version Revision [8] (Low Word) | UINT32 |  |  |
| 255 | Rectifier Version Revision [9] (High Word) | UINT32 |  |  |
| 256 | Rectifier Version Revision [9] (Low Word) | UINT32 |  |  |
| 257 | Rectifier Version Revision [10] (High Word) | UINT32 |  |  |
| 258 | Rectifier Version Revision [10] (Low Word) | UINT32 |  |  |
| 259 | Rectifier Version Revision [11] (High Word) | UINT32 |  |  |
| 260 | Rectifier Version Revision [11] (Low Word) | UINT32 |  |  |
| 261 | Rectifier Version Revision [12] (High Word) | UINT32 |  |  |
| 262 | Rectifier Version Revision [12] (Low Word) | UINT32 |  |  |
| 263 | Rectifier Version Revision [13] (High Word) | UINT32 |  |  |
| 264 | Rectifier Version Revision [13] (Low Word) | UINT32 |  |  |
| 265 | Rectifier Version Revision [14] (High Word) | UINT32 |  |  |
| 266 | Rectifier Version Revision [14] (Low Word) | UINT32 |  |  |
| 267 | Rectifier Version Revision [15] (High Word) | UINT32 |  |  |
| 268 | Rectifier Version Revision [15] (Low Word) | UINT32 |  |  |
| 270 | Reserved | UINT16 |  |  |
| 271 | Reserved | UINT16 |  |  |
| 272 | User Alarms [1 to 16] | UINT16 |  | Packed Boolean format |
| 273 | User Alarms [17 to 32] | UINT16 |  | Packed Boolean format |
| 274 | User Alarms [33 to 48] | UINT16 |  | Packed Boolean format |
| 275 | User Alarms [49 to 64] | UINT16 |  | Packed Boolean format |
| 276 | Group Alarms [1 to 16] | UINT16 |  | Packed Boolean format |
| 277 | Group Alarms [17 to 32] | UINT16 |  | Packed Boolean format |
| 278 | Group Alarms [33 to 48] | UINT16 |  | Packed Boolean format |
| 279 | Group Alarms [49 to 64] | UINT16 |  | Packed Boolean format |
| 280 | Analog Alarms [1 to 16] | UINT16 |  | Packed Boolean format |
| 281 | Reserved | UINT16 |  |  |
| 282 | Reserved | UINT16 |  |  |
| 283 | Reserved | UINT16 |  |  |
| 284 | Reserved | UINT16 |  |  |
| 285 | Reserved | UINT16 |  |  |
| 286 | Reserved | UINT16 |  |  |
| 287 | Reserved | UINT16 |  |  |

## Parsing Rules

- Treat the raw payload as the source of truth and preserve it in logs.
- Parse M2000 data only when the raw/canonical message context identifies the payload as supported M2000 input-register telemetry.
- Use the configured CSV mapping as the source of register metadata.
- Ignore unsupported or unmapped node keys safely after logging enough context to troubleshoot.
- Decode multi-register values according to the mapping `Data-type`.
- Apply supported formula scaling such as divide by 10, multiply factor, and divide factor when mapping metadata defines it.
- Enrich mapped readings with protocol, function code, register address, original node name, data type, friendly display name, and unit.
- Keep Modbus TCP polling behavior separate from raw MQTT message handling.
- Malformed payloads should produce an error log entry and be skipped safely.
- Do not add holding-register reads, writes, remote command handling, or remote configuration in this phase.
