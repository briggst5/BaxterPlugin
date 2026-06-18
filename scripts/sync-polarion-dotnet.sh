#!/usr/bin/env bash
# Sync PolarionMCP main branch into baxter-polarion and rebuild binaries.
set -euo pipefail

SOURCE_REPO="${SOURCE_REPO:-https://github.com/briggst5/PolarionMCP.git}"
SOURCE_BRANCH="${SOURCE_BRANCH:-main}"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PLUGIN_DIR="${ROOT}/plugins/baxter-polarion"
TARGET_DIR="${PLUGIN_DIR}/mcp-servers"
BUILD_SCRIPT="${PLUGIN_DIR}/scripts/build-polarion-binaries.sh"

if [[ ! -d "${PLUGIN_DIR}" ]]; then
  echo "Plugin directory not found: ${PLUGIN_DIR}" >&2
  exit 1
fi

if [[ ! -x "${BUILD_SCRIPT}" ]]; then
  echo "Build script is missing or not executable: ${BUILD_SCRIPT}" >&2
  exit 1
fi

if ! command -v git >/dev/null 2>&1; then
  echo "git is required." >&2
  exit 1
fi

if ! command -v rsync >/dev/null 2>&1; then
  echo "rsync is required." >&2
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET 8 SDK is required to build Polarion MCP binaries." >&2
  exit 1
fi

tmp="$(mktemp -d)"
cleanup() {
  rm -rf "${tmp}"
}
trap cleanup EXIT

echo "==> Cloning ${SOURCE_REPO} (${SOURCE_BRANCH})"
git clone --depth 1 --branch "${SOURCE_BRANCH}" "${SOURCE_REPO}" "${tmp}/PolarionMCP"

echo "==> Syncing source into ${TARGET_DIR}"
mkdir -p "${TARGET_DIR}"
rsync -a --delete \
  --exclude=".git" \
  --exclude=".github" \
  "${tmp}/PolarionMCP/" "${TARGET_DIR}/"

echo "==> Building bundled linux-x64 and win-x64 binaries"
"${BUILD_SCRIPT}"

echo "==> Optional validation"
"${ROOT}/scripts/validate-plugin.sh"

echo ""
echo "Done. Review changes, then commit updated mcp-servers and bin artifacts."
