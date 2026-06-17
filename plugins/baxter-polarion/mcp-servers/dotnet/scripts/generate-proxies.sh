#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${POLARION_URL:-}" ]]; then
  echo "POLARION_URL is required, e.g. https://polarion.example.local/polarion" >&2
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
CLIENT_DIR="$ROOT_DIR/PolarionMcp.Client"

echo "Generating SOAP proxies into $CLIENT_DIR/ConnectedServices ..."

cd "$CLIENT_DIR"

dotnet tool restore

dotnet dotnet-svcutil "${POLARION_URL%/}/ws/services/SessionWebService?wsdl" \
  --outputDir ConnectedServices/SessionWebService \
  --namespace "*,PolarionMcp.Client.ConnectedServices.SessionWebService"

dotnet dotnet-svcutil "${POLARION_URL%/}/ws/services/TrackerWebService?wsdl" \
  --outputDir ConnectedServices/TrackerWebService \
  --namespace "*,PolarionMcp.Client.ConnectedServices.TrackerWebService"

dotnet dotnet-svcutil "${POLARION_URL%/}/ws/services/TestManagementWebService?wsdl" \
  --outputDir ConnectedServices/TestManagementWebService \
  --namespace "*,PolarionMcp.Client.ConnectedServices.TestManagementWebService"

echo "Proxy generation complete."
