#!/usr/bin/env bash
# Wiki sync loop helper — emits AGENT_LOOP_TICK for Cursor /loop integration.
# Usage: bash scripts/wiki-sync-loop.sh [interval_seconds]
# Default interval: 86400 (1 day)

set -euo pipefail

INTERVAL="${1:-86400}"
REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PURPOSE="wiki_sync"

cd "$REPO_ROOT"

echo "Wiki sync loop armed: every ${INTERVAL}s in ${REPO_ROOT}"
echo "First tick in ${INTERVAL}s; run sync manually now if needed."

while true; do
  sleep "$INTERVAL"
  echo "AGENT_LOOP_TICK_${PURPOSE} {\"prompt\":\"Run wiki-sync skill: execute python3 scripts/sync_wiki.py, report pages synced and any errors. If script fails on auth, use Azure DevOps MCP wiki tools instead.\"}"
done
