#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${1:-http://localhost:5056}"

echo "Checking live endpoint: $BASE_URL/health/live"
curl --fail --silent --show-error "$BASE_URL/health/live" >/dev/null

echo "Checking ready endpoint: $BASE_URL/health/ready"
curl --fail --silent --show-error "$BASE_URL/health/ready" >/dev/null

echo "Smoke check passed for $BASE_URL"
