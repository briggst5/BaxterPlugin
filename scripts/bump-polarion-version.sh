#!/usr/bin/env bash
# Bump baxter-polarion plugin version and prepend changelog entry.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CURSOR_MANIFEST="${ROOT}/plugins/baxter-polarion/.cursor-plugin/plugin.json"
COPILOT_MANIFEST="${ROOT}/plugins/baxter-polarion/.plugin/plugin.json"
CHANGELOG="${ROOT}/plugins/baxter-polarion/CHANGELOG.md"
UPSTREAM_SHA="${1:-}"

if [[ ! -f "${CURSOR_MANIFEST}" || ! -f "${COPILOT_MANIFEST}" ]]; then
  echo "Missing baxter-polarion plugin manifests." >&2
  exit 1
fi

if [[ ! -f "${CHANGELOG}" ]]; then
  echo "Missing changelog: ${CHANGELOG}" >&2
  exit 1
fi

python3 - "${CURSOR_MANIFEST}" "${COPILOT_MANIFEST}" "${CHANGELOG}" "${UPSTREAM_SHA}" <<'PY'
import json
import re
import sys
from pathlib import Path

cursor_path = Path(sys.argv[1])
copilot_path = Path(sys.argv[2])
changelog_path = Path(sys.argv[3])
upstream_sha = sys.argv[4].strip()

cursor = json.loads(cursor_path.read_text())
copilot = json.loads(copilot_path.read_text())

cursor_version = cursor.get("version")
copilot_version = copilot.get("version")
if not cursor_version or not copilot_version:
    raise SystemExit("Both plugin manifests must include a version.")
if cursor_version != copilot_version:
    raise SystemExit(
        f"Manifest versions differ: cursor={cursor_version}, copilot={copilot_version}"
    )

match = re.fullmatch(r"(\d+)\.(\d+)\.(\d+)", cursor_version)
if not match:
    raise SystemExit(f"Unsupported version format: {cursor_version}")

major, minor, patch = (int(v) for v in match.groups())
next_version = f"{major}.{minor}.{patch + 1}"

cursor["version"] = next_version
copilot["version"] = next_version
cursor_path.write_text(json.dumps(cursor, indent=2) + "\n")
copilot_path.write_text(json.dumps(copilot, indent=2) + "\n")

changelog = changelog_path.read_text()
sha_note = f" (upstream `{upstream_sha[:12]}`)" if upstream_sha else ""
entry = (
    f"## {next_version}\n\n"
    f"- Synced PolarionMCP dotnet branch and refreshed bundled linux-x64/win-x64 binaries{sha_note}\n\n"
)

if changelog.startswith("# Changelog\n\n"):
    updated = "# Changelog\n\n" + entry + changelog[len("# Changelog\n\n") :]
else:
    updated = f"# Changelog\n\n{entry}{changelog}"

changelog_path.write_text(updated)
print(next_version)
PY
