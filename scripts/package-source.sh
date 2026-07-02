#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT="$ROOT/_handover"
STAMP="$(date +%Y%m%d-%H%M%S)"
NAME="signaleye-device-gateway-source-$STAMP"

mkdir -p "$OUT"

rsync -a \
  --exclude ".git" \
  --exclude "bin" \
  --exclude "obj" \
  --exclude ".vs" \
  --exclude ".vscode" \
  --exclude ".idea" \
  --exclude "logs" \
  --exclude "*.log" \
  --exclude "_handover" \
  "$ROOT/" "$OUT/$NAME/"

(
  cd "$OUT"
  zip -qr "$NAME.zip" "$NAME"
)

echo "$OUT/$NAME.zip"
