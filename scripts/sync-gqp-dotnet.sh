#!/usr/bin/env bash
# Sync GQP MCP source into baxter-gqp and rebuild bundled binaries.
set -euo pipefail

SOURCE_REPO="${SOURCE_REPO:-}"
SOURCE_BRANCH="${SOURCE_BRANCH:-main}"
SOURCE_PATH="${SOURCE_PATH:-}"

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PLUGIN_DIR="${ROOT}/plugins/baxter-gqp"
TARGET_DIR="${PLUGIN_DIR}/mcp-servers"
BUILD_SCRIPT="${PLUGIN_DIR}/scripts/build-gqp-binaries.sh"

if [[ ! -d "${PLUGIN_DIR}" ]]; then
  echo "Plugin directory not found: ${PLUGIN_DIR}" >&2
  exit 1
fi

if [[ ! -x "${BUILD_SCRIPT}" ]]; then
  echo "Build script is missing or not executable: ${BUILD_SCRIPT}" >&2
  exit 1
fi

if ! command -v rsync >/dev/null 2>&1; then
  echo "rsync is required." >&2
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET 8 SDK is required to build GQP MCP binaries." >&2
  exit 1
fi

tmp=""
cleanup() {
  if [[ -n "${tmp}" && -d "${tmp}" ]]; then
    rm -rf "${tmp}"
  fi
}
trap cleanup EXIT

sync_from_path() {
  local source="$1"
  if [[ ! -d "${source}/mcp-servers/dotnet" ]]; then
    echo "GQP dotnet source not found: ${source}/mcp-servers/dotnet" >&2
    exit 1
  fi

  echo "==> Syncing dotnet source from ${source}"
  mkdir -p "${TARGET_DIR}"
  rsync -a --delete \
    --exclude="bin" \
    --exclude="obj" \
    "${source}/mcp-servers/dotnet/" "${TARGET_DIR}/dotnet/"

  if [[ -f "${source}/gqp-mcp.env.example" ]]; then
    cp "${source}/gqp-mcp.env.example" "${TARGET_DIR}/gqp-mcp.env.example"
  fi

  if [[ -f "${source}/mcp-servers/README.md" ]]; then
    cp "${source}/mcp-servers/README.md" "${TARGET_DIR}/README.md"
  fi
}

if [[ -n "${SOURCE_PATH}" ]]; then
  sync_from_path "${SOURCE_PATH}"
elif [[ -n "${SOURCE_REPO}" ]]; then
  if ! command -v git >/dev/null 2>&1; then
    echo "git is required when SOURCE_REPO is set." >&2
    exit 1
  fi
  tmp="$(mktemp -d)"
  echo "==> Cloning ${SOURCE_REPO} (${SOURCE_BRANCH})"
  git clone --depth 1 --branch "${SOURCE_BRANCH}" "${SOURCE_REPO}" "${tmp}/GQP"
  sync_from_path "${tmp}/GQP"
else
  default_path="$(cd "${ROOT}/../GQP" 2>/dev/null && pwd || true)"
  if [[ -z "${default_path}" || ! -d "${default_path}" ]]; then
    echo "Set SOURCE_PATH or SOURCE_REPO, or clone GQP next to BaxterPlugin (../GQP)." >&2
    exit 1
  fi
  sync_from_path "${default_path}"
fi

echo "==> Building bundled linux-x64 and win-x64 binaries"
"${BUILD_SCRIPT}"

echo "==> Optional validation"
"${ROOT}/scripts/validate-plugin.sh"

echo ""
echo "Done. Review changes, then commit updated mcp-servers and bin artifacts."
