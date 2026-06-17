#!/usr/bin/env bash
# Launch a vendored Python MCP server with uv (lazy dependency sync).
set -euo pipefail

SERVER_NAME="${1:?Usage: run-mcp-server.sh <server-name>}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PLUGIN_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
SERVER_DIR="${PLUGIN_ROOT}/mcp-servers/${SERVER_NAME}"

if [[ ! -d "${SERVER_DIR}" ]]; then
  echo "MCP server directory not found: ${SERVER_DIR}" >&2
  exit 1
fi

if ! command -v uv >/dev/null 2>&1; then
  echo "uv is not installed. Run scripts/bootstrap-dev-machine.sh first." >&2
  exit 1
fi

exec uv --directory "${SERVER_DIR}" run server.py
