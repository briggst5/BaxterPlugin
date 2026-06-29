#!/usr/bin/env bash
# Validate Baxter marketplace plugins before release.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ERRORS=0

error() {
  echo "ERROR: $*" >&2
  ERRORS=$((ERRORS + 1))
}

warn() {
  echo "WARN: $*" >&2
}

check_json() {
  python3 -m json.tool "$1" >/dev/null 2>&1 || error "Invalid JSON: $1"
}

check_frontmatter_field() {
  local file="$1"
  local field="$2"
  if ! grep -q "^${field}:" "$file" 2>/dev/null; then
    error "Missing frontmatter field '${field}' in ${file}"
  fi
}

check_no_absolute_paths() {
  local file="$1"
  if grep -E '"/|"/Users|"/home|"\.\./' "$file" >/dev/null 2>&1; then
    error "Absolute or parent paths in ${file}"
  fi
}

validate_plugin() {
  local plugin_dir="$1"
  local plugin_name
  plugin_name="$(basename "$plugin_dir")"
  echo "==> Validating plugin: ${plugin_name}"

  local cursor_manifest="${plugin_dir}/.cursor-plugin/plugin.json"
  local copilot_manifest="${plugin_dir}/.plugin/plugin.json"

  if [[ ! -f "$cursor_manifest" ]]; then
    error "Missing ${cursor_manifest}"
    return
  fi
  check_json "$cursor_manifest"
  check_no_absolute_paths "$cursor_manifest"

  if [[ -f "$copilot_manifest" ]]; then
    check_json "$copilot_manifest"
    check_no_absolute_paths "$copilot_manifest"
  else
    warn "No Copilot manifest at ${copilot_manifest}"
  fi

  if [[ -d "${plugin_dir}/skills" ]]; then
    shopt -s nullglob
    for skill in "${plugin_dir}"/skills/*/SKILL.md; do
      check_frontmatter_field "$skill" "name"
      check_frontmatter_field "$skill" "description"
    done
    shopt -u nullglob
  fi

  if [[ -d "${plugin_dir}/rules" ]]; then
    shopt -s nullglob
    for rule in "${plugin_dir}"/rules/*.{mdc,md}; do
      check_frontmatter_field "$rule" "description"
    done
    shopt -u nullglob
  fi

  if [[ -d "${plugin_dir}/agents" ]]; then
    shopt -s nullglob
    for agent in "${plugin_dir}"/agents/*.md; do
      check_frontmatter_field "$agent" "name"
      check_frontmatter_field "$agent" "description"
    done
    shopt -u nullglob
  fi

  for mcp_file in "${plugin_dir}/.mcp.json" "${plugin_dir}/.mcp.copilot.json"; do
    if [[ -f "$mcp_file" ]]; then
      check_json "$mcp_file"
      check_no_absolute_paths "$mcp_file"
    fi
  done

  if [[ -d "${plugin_dir}/mcp-servers" ]]; then
    shopt -s nullglob
    local found_server=false
    for server_dir in "${plugin_dir}"/mcp-servers/*/; do
      if [[ ! -f "${server_dir}pyproject.toml" && ! -f "${server_dir}requirements.txt" ]]; then
        continue
      fi
      found_server=true
      if [[ ! -f "${server_dir}server.py" ]]; then
        warn "No server.py in ${server_dir} (may use package entrypoint instead)"
      fi
    done
    if [[ -f "${plugin_dir}/mcp-servers/pyproject.toml" || -f "${plugin_dir}/mcp-servers/requirements.txt" ]]; then
      found_server=true
      if [[ ! -f "${plugin_dir}/mcp-servers/polarion_mcp/server.py" && ! -f "${plugin_dir}/mcp-servers/server.py" ]]; then
        warn "No server entrypoint found under ${plugin_dir}/mcp-servers/"
      fi
    fi
    if [[ -f "${plugin_dir}/mcp-servers/dotnet/PolarionMcp.sln" ]]; then
      found_server=true
    fi
    if [[ "${found_server}" == false ]]; then
      warn "mcp-servers/ has no recognized MCP project metadata: ${plugin_dir}/mcp-servers/"
    fi
    shopt -u nullglob
    if [[ ! -x "${plugin_dir}/scripts/run-mcp-server.sh" ]]; then
      if grep -q 'run-mcp-server.sh' "${plugin_dir}/.mcp.json" "${plugin_dir}/.mcp.copilot.json" 2>/dev/null; then
        error "Missing or non-executable ${plugin_dir}/scripts/run-mcp-server.sh (referenced in MCP config)"
      fi
    fi
  fi

  for manifest in "$cursor_manifest" "$copilot_manifest"; do
    [[ -f "$manifest" ]] || continue
    python3 - "$manifest" <<'PY'
import json
import sys
from pathlib import Path

manifest = json.loads(Path(sys.argv[1]).read_text())
plugin_dir = Path(sys.argv[1]).parent.parent
for key in ("skills", "rules", "agents", "mcpServers", "hooks"):
    if key not in manifest:
        continue
    values = manifest[key]
    if isinstance(values, str):
        values = [values]
    elif isinstance(values, dict):
        continue
    for rel in values:
        rel_path = rel.removeprefix("./")
        target = plugin_dir / rel_path
        if not target.exists():
            print(f"ERROR: Declared path missing in manifest: {rel}", file=sys.__stderr__)
            sys.exit(1)
PY
    if [[ $? -ne 0 ]]; then
      ERRORS=$((ERRORS + 1))
    fi
  done

  if [[ "${plugin_name}" == "baxter-polarion" ]]; then
    local linux_bin="${plugin_dir}/bin/linux-x64/polarion-mcp"
    local win_bin="${plugin_dir}/bin/win-x64/polarion-mcp.exe"
    if [[ ! -f "${linux_bin}" ]]; then
      error "Missing bundled Linux binary: ${linux_bin}"
    fi
    if [[ ! -f "${win_bin}" ]]; then
      error "Missing bundled Windows binary: ${win_bin}"
    fi
  fi
}

echo "==> Validating Cursor marketplace manifest"
MARKETPLACE="${ROOT}/.cursor-plugin/marketplace.json"
COPILOT_MARKETPLACE="${ROOT}/.github/plugin/marketplace.json"
if [[ ! -f "$MARKETPLACE" ]]; then
  error "Missing ${MARKETPLACE}"
else
  check_json "$MARKETPLACE"
  while IFS= read -r source; do
    plugin_dir="${ROOT}/${source#./}"
    if [[ ! -d "$plugin_dir" ]]; then
      error "Marketplace plugin source not found: ${plugin_dir}"
    else
      validate_plugin "$plugin_dir"
    fi
  done < <(python3 - "$MARKETPLACE" <<'PY'
import json, sys
from pathlib import Path
data = json.loads(Path(sys.argv[1]).read_text())
for p in data.get("plugins", []):
    print(p["source"])
PY
)
fi

echo "==> Validating Copilot (VS Code) marketplace manifest"
if [[ ! -f "$COPILOT_MARKETPLACE" ]]; then
  error "Missing ${COPILOT_MARKETPLACE}"
else
  check_json "$COPILOT_MARKETPLACE"
  while IFS= read -r source; do
    plugin_dir="${ROOT}/${source#./}"
    if [[ ! -d "$plugin_dir" ]]; then
      error "Copilot marketplace plugin source not found: ${plugin_dir}"
    elif [[ ! -f "${plugin_dir}/.plugin/plugin.json" ]]; then
      error "Copilot manifest missing for marketplace plugin: ${plugin_dir}/.plugin/plugin.json"
    fi
  done < <(python3 - "$COPILOT_MARKETPLACE" <<'PY'
import json, sys
from pathlib import Path
data = json.loads(Path(sys.argv[1]).read_text())
for p in data.get("plugins", []):
    print(p["source"])
PY
)

  if [[ -f "$MARKETPLACE" ]]; then
    python3 - "$MARKETPLACE" "$COPILOT_MARKETPLACE" <<'PY' || error "Cursor and Copilot marketplaces list different plugins"
import json, sys
from pathlib import Path
cursor = json.loads(Path(sys.argv[1]).read_text())
copilot = json.loads(Path(sys.argv[2]).read_text())
cursor_plugins = {p["source"].removeprefix("./") for p in cursor.get("plugins", [])}
copilot_plugins = {p["source"].removeprefix("./") for p in copilot.get("plugins", [])}
sys.exit(0 if cursor_plugins == copilot_plugins else 1)
PY
  fi
fi

if [[ $ERRORS -gt 0 ]]; then
  echo ""
  echo "Validation failed with ${ERRORS} error(s)."
  exit 1
fi

echo ""
echo "Validation passed."
