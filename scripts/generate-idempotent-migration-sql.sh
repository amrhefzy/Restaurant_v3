#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUTPUT_DIR="$ROOT_DIR/artifacts/sql"
OUTPUT_FILE="$OUTPUT_DIR/migrations-idempotent.sql"

mkdir -p "$OUTPUT_DIR"
cd "$ROOT_DIR"

dotnet tool restore

dotnet ef migrations script --idempotent \
  --project RestaurantManagement.Infrastructure/RestaurantManagement.Infrastructure.csproj \
  --startup-project RestaurantManagement.Web/RestaurantManagement.Web.csproj \
  --output "$OUTPUT_FILE"

echo "Generated: $OUTPUT_FILE"
