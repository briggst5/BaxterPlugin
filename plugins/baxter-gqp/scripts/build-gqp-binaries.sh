#!/usr/bin/env bash
# Build self-contained GQP MCP binaries into this plugin's bin/ directory.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOTNET="${ROOT}/mcp-servers/dotnet"
SOLUTION="${DOTNET}/GqpMcp.sln"
PROJECT="${DOTNET}/GqpMcp.Server/GqpMcp.Server.csproj"
OUT_ROOT="${ROOT}/bin"

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET SDK is required to build GQP MCP binaries." >&2
  exit 1
fi

echo "==> Restore GQP MCP solution"
dotnet restore "${SOLUTION}"

echo "==> Build GQP MCP solution"
dotnet build "${SOLUTION}" -c Release --no-restore

publish_rid() {
  local rid="$1"
  local out="${OUT_ROOT}/${rid}"
  mkdir -p "${out}"

  echo "==> Publish ${rid}"
  dotnet publish "${PROJECT}" \
    -c Release \
    -r "${rid}" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -o "${out}"
}

publish_rid "linux-x64"
publish_rid "osx-arm64"
publish_rid "osx-x64"
publish_rid "win-x64"

echo ""
echo "Binaries published:"
echo "  ${OUT_ROOT}/linux-x64/gqp-mcp"
echo "  ${OUT_ROOT}/osx-arm64/gqp-mcp"
echo "  ${OUT_ROOT}/osx-x64/gqp-mcp"
echo "  ${OUT_ROOT}/win-x64/gqp-mcp.exe"
