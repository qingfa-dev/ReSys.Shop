#!/usr/bin/env bash
# wait-for-pg.sh — block until PostgreSQL is accepting connections
#
# Usage:
#   ./infra/postgres/wait-for-pg.sh              # uses env vars
#   ./infra/postgres/wait-for-pg.sh -- benchmark benchmark  # then run a command
#
# Environment variables (match docker-compose.yml):
#   PGHOST      default: localhost
#   PGPORT      default: 5432
#   PGUSER      default: benchmark
#   PGDATABASE  default: benchmark
#   PGPASSWORD  (set in env)
#   PG_TIMEOUT  max seconds to wait, default: 60

set -euo pipefail

HOST="${PGHOST:-localhost}"
PORT="${PGPORT:-5432}"
USER="${PGUSER:-benchmark}"
DB="${PGDATABASE:-benchmark}"
TIMEOUT="${PG_TIMEOUT:-60}"

echo "Waiting for PostgreSQL at ${HOST}:${PORT} (db=${DB}, user=${USER}) …"

elapsed=0
until pg_isready -h "$HOST" -p "$PORT" -U "$USER" -d "$DB" -q 2>/dev/null; do
    if [ "$elapsed" -ge "$TIMEOUT" ]; then
        echo "ERROR: PostgreSQL did not become ready within ${TIMEOUT}s"
        exit 1
    fi
    sleep 2
    elapsed=$((elapsed + 2))
    echo "  … still waiting (${elapsed}s)"
done

echo "PostgreSQL is ready (waited ${elapsed}s)"

# If extra arguments were passed after "--", execute them
shift_marker=0
for arg in "$@"; do
    if [ "$arg" = "--" ]; then
        shift_marker=1
        shift
        break
    fi
    shift
done

if [ $# -gt 0 ]; then
    exec "$@"
fi
