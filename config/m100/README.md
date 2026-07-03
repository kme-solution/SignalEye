# PUSR M100 Configuration

These files are deployed to or configured on the M100 gateway. They are distinct from SignalEye's enriched runtime mappings under `config/modbus`.

| File | Purpose |
|---|---|
| `mappings/m2000.csv` | M100 Modbus TCP polling/import configuration for the connected M2000. |
| `templates/telemetry.json` | Minified MQTT JSON template. It is 667 UTF-8 bytes and must remain below the M100 limit of 2,048 bytes. |

The template uses one top-level object per connected device. The object key is the device instance key, while `d` is the lowercase server mapping profile:

```json
{
  "device01": {
    "p": 1,
    "s": 1,
    "d": "m2000",
    "fc": 4,
    "m": {
      "node00": "node00"
    }
  }
}
```

Always deploy the minified file rather than the formatted documentation example. Add other devices as sibling top-level objects and use unique M100 node placeholders for their metrics.

SignalEye enriches these raw values with display names, units, formulas, and register metadata from `config/modbus/edge-EN.csv`.
