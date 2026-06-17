#!/usr/bin/env bash
# Build self-contained .NET Polarion MCP binaries for Linux and Windows.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SERVER="${ROOT}/mcp-servers"
SOLUTION="${SERVER}/dotnet/PolarionMcp.sln"
PROJECT="${SERVER}/dotnet/PolarionMcp.Server/PolarionMcp.Server.csproj"

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET 8 SDK is required to build Polarion MCP binaries." >&2
  exit 1
fi

echo "==> Restore .NET solution"
dotnet restore "${SOLUTION}"

echo "==> Build .NET solution"
dotnet build "${SOLUTION}" -c Release --no-restore

publish_rid() {
  local rid="$1"
  local out="${ROOT}/bin/${rid}"
  mkdir -p "${out}"

  echo "==> Publish ${rid}"
  dotnet publish "${PROJECT}" \
    -c Release \
    -r "${rid}" \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:AssemblyName=polarion-mcp \
    -o "${out}"
}

publish_rid "linux-x64"
publish_rid "win-x64"

echo ""
echo "Binaries published:"
echo "  ${ROOT}/bin/linux-x64/polarion-mcp"
echo "  ${ROOT}/bin/win-x64/polarion-mcp.exe"
echo ""
echo "Commit these files so plugin users do not compile locally."
