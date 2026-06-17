#!/usr/bin/env bash
# Add a vendored MCP server entry to Cursor and Copilot MCP config files.
set -euo pipefail

PLUGIN="${1:?Usage: add-mcp-entry.sh <plugin-name> <server-dir-name> [mcp-key]}"
SERVER_DIR="${2:?Usage: add-mcp-entry.sh <plugin-name> <server-dir-name> [mcp-key]}"
MCP_KEY="${3:-$SERVER_DIR}"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PLUGIN_DIR="${ROOT}/plugins/${PLUGIN}"
SERVER_PATH="${PLUGIN_DIR}/mcp-servers/${SERVER_DIR}"

if [[ ! -d "${SERVER_PATH}" ]]; then
  echo "Server directory not found: ${SERVER_PATH}" >&2
  echo "Copy the MCP project there first (see CONTRIBUTING.md)." >&2
  exit 1
fi

python3 - "${PLUGIN_DIR}" "${MCP_KEY}" "${SERVER_DIR}" <<'PY'
import json
import sys
from pathlib import Path

plugin_dir = Path(sys.argv[1])
mcp_key = sys.argv[2]
server_dir = sys.argv[3]
launcher = "scripts/run-mcp-server.sh"

cursor_path = plugin_dir / ".mcp.json"
copilot_path = plugin_dir / ".mcp.copilot.json"

entry_cursor = {
    "command": "bash",
    "args": [f"${{CURSOR_PLUGIN_ROOT}}/{launcher}", server_dir],
}
entry_copilot = {
    "command": "bash",
    "args": [f"${{PLUGIN_ROOT}}/{launcher}", server_dir],
}

for path, entry in ((cursor_path, entry_cursor), (copilot_path, entry_copilot)):
    data = {"mcpServers": {}}
    if path.exists():
        data = json.loads(path.read_text())
    servers = data.setdefault("mcpServers", {})
    if mcp_key in servers:
        print(f"Updating existing entry: {mcp_key} in {path.name}")
    else:
        print(f"Adding entry: {mcp_key} to {path.name}")
    servers[mcp_key] = entry
    path.write_text(json.dumps(data, indent=2) + "\n")

print("Done. Run ./scripts/validate-plugin.sh")
PY
