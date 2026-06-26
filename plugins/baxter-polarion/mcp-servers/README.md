# PolarionMCP (.NET)

MCP server for Polarion ALM using SOAP/WSDL services.

## Status

- Runtime: `.NET 8`
- Transport: MCP stdio
- Platforms (v1): `win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`
- API surface: Polarion SOAP only (no REST)

## Repository Layout

```text
dotnet/
  PolarionMcp.sln
  PolarionMcp.Server/         MCP stdio host and tool definitions
  PolarionMcp.Client/         Polarion SOAP client + session handling
  PolarionMcp.Client.Tests/   Unit tests
  PolarionMcp.Smoke/          Live smoke runner
  scripts/generate-proxies.sh Regenerate SOAP proxies from WSDL
docs/
  OPERATIONS.md               Installation/config/troubleshooting guide
polarion-mcp.env.example      Environment configuration template
```

## Tools Exposed

- `get_work_item`
- `set_work_item_status`
- `list_work_item_workflow_actions`
- `get_work_item_links`
- `add_work_item_link`
- `remove_work_item_link`
- `get_work_item_raw_fields`
- `query_work_items`
- `list_documents`
- `list_document_work_items`
- `list_configuration_srs_inventory`
- `get_document_text`
- `get_document_work_items`
- `list_work_item_comments`
- `add_work_item_comment`
- `list_reviewers`
- `add_reviewer`
- `remove_reviewer`
- `reset_review_status`
- `create_test_result_record`

## Build and Test

```bash
dotnet restore dotnet/PolarionMcp.sln
dotnet build dotnet/PolarionMcp.sln -c Release
dotnet test dotnet/PolarionMcp.Client.Tests/PolarionMcp.Client.Tests.csproj -c Release
```

## Run the Server

```bash
dotnet run --project dotnet/PolarionMcp.Server/PolarionMcp.Server.csproj -c Release
```

For binary deployment, publish for the target runtime identifier (RID). Supported
RIDs: `win-x64`, `linux-x64`, `osx-x64` (Intel Macs), `osx-arm64` (Apple Silicon).

```bash
dotnet publish dotnet/PolarionMcp.Server/PolarionMcp.Server.csproj \
  -c Release \
  -r osx-arm64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:AssemblyName=polarion-mcp \
  -o artifacts/osx-arm64
```

## Configuration

Server reads `~/.config/polarion-mcp.env` on startup.

Required:

- `POLARION_URL`
- `POLARION_USER`
- one of `POLARION_PASSWORD` or `POLARION_PAT`

Common optional settings:

- `POLARION_PROJECT`
- `POLARION_TLS_SKIP_VERIFY`
- `POLARION_TLS_CA_FILE`
- `POLARION_SESSION_MAX_RETRIES`
- `POLARION_SESSION_IDLE_REFRESH_SECONDS`
- `POLARION_MCP_MAX_CALLS`

See `polarion-mcp.env.example` and `docs/OPERATIONS.md` for full guidance.

## Cursor MCP Config

`.cursor/mcp.json`:

```json
{
  "mcpServers": {
    "polarion-mcp": {
      "command": "/absolute/path/to/polarion-mcp",
      "args": []
    }
  }
}
```

## Regenerate SOAP Proxies

```bash
export POLARION_URL="https://your-polarion/polarion"
bash dotnet/scripts/generate-proxies.sh
```

## Smoke Runner

Run the local parity smoke harness:

```bash
dotnet run --project dotnet/PolarionMcp.Smoke/PolarionMcp.Smoke.csproj -c Release
```

Optional mutable-test controls:

- `POLARION_MUTABLE_WORK_ITEM_ID` (default: `PLT1-2668`)
- `POLARION_TEST_RUN_ID` (required only for `create_test_result_record` smoke)

## Operations Guide

Installation, configuration, troubleshooting, and recovery:

- [`docs/OPERATIONS.md`](docs/OPERATIONS.md)
