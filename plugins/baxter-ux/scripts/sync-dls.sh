#!/usr/bin/env bash
# Sync Baxter Nexus DLS reference material into the baxter-ux plugin.
set -euo pipefail

PLUGIN_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DLS_SRC="${DLS_SRC:-/mnt/d/dls}"

if [[ ! -d "$DLS_SRC/design-system" ]]; then
  echo "ERROR: design-system not found at ${DLS_SRC}/design-system" >&2
  exit 1
fi

echo "==> Syncing Zero Height exports from ${DLS_SRC}"
rsync -a --delete "${DLS_SRC}/design-system/" "${PLUGIN_ROOT}/design-system/"

if compgen -G "${DLS_SRC}/nexus-dls-"*.mdc >/dev/null; then
  echo "==> Syncing Nexus DLS rules"
  cp "${DLS_SRC}"/nexus-dls-*.mdc "${PLUGIN_ROOT}/rules/"
  sed -i 's|docs/design-system/|design-system/|g' "${PLUGIN_ROOT}/rules/"nexus-dls-*.mdc
else
  echo "WARN: no nexus-dls-*.mdc rules found in ${DLS_SRC}" >&2
fi

if [[ -d "${PLUGIN_ROOT}/figma/incoming" ]] && compgen -G "${PLUGIN_ROOT}/figma/incoming/"* >/dev/null; then
  echo "==> Promoting Figma files from figma/incoming/"
  mkdir -p "${PLUGIN_ROOT}/figma"
  rsync -a "${PLUGIN_ROOT}/figma/incoming/" "${PLUGIN_ROOT}/figma/"
  rm -rf "${PLUGIN_ROOT}/figma/incoming/"*
fi

echo "==> Done. Design-system files: $(find "${PLUGIN_ROOT}/design-system" -type f | wc -l)"
echo "    Rules: $(find "${PLUGIN_ROOT}/rules" -name 'nexus-dls-*.mdc' | wc -l) nexus-dls rules"
