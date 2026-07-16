#!/usr/bin/env bash
set -euo pipefail

# Wait for pgvector PostgreSQL to accept connections
RETRIES=30
SLEEP=2

for i in $(seq 1 $RETRIES); do
  if podman exec pgvector-benchmark pg_isready -U benchmark -d benchmark &>/dev/null; then
    echo "PostgreSQL ready."
    exit 0
  fi
  echo "Waiting for PostgreSQL ($i/$RETRIES)..."
  sleep $SLEEP
done

echo "ERROR: PostgreSQL did not become ready after ${RETRIES} retries."
exit 1
