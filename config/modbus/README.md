# Modbus Mapping Configuration

Runtime Modbus mapping files live here.

Expected first mapping source:

```text
config/modbus/edge-EN.csv
```

This CSV is the active runtime mapping source for M2000 telemetry. It should contain only the nodes SignalEye wants to retrieve from the M2000, not the full M2000 register table.

Each included node should provide the PUSR M100 node mapping plus the friendly name, unit, data type, register metadata, and formula needed by `device-gateway-service`.

This is a SignalEye server configuration, not the original M100 import file. The gateway polling CSV and MQTT JSON template are documented in [`config/m100/README.md`](../m100/README.md).
