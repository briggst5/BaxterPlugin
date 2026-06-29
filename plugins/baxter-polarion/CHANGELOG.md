# Changelog

## 0.2.1

- Synced PolarionMCP main branch and refreshed bundled linux-x64/win-x64 binaries (upstream `fe0dc9d0252a`)

## 0.2.0

- Add macOS support: bundled `osx-x64` (Intel) and `osx-arm64` (Apple Silicon) binaries
- Launcher selects the macOS binary by architecture (`darwin` + `arm64`/`x64`)
- `build-polarion-binaries.sh` publishes macOS RIDs alongside linux-x64/win-x64
- Document macOS install/verify (incl. Gatekeeper quarantine step) in INSTALL/README/OPERATIONS

## 0.1.3

- Document Polarion PAT generation and 90-day expiry in INSTALL, README, and OPERATIONS
- Note PAT renewal in `polarion-mcp.env.example`

## 0.1.2

- Synced PolarionMCP `main` at `fe0dc9d` and refreshed bundled linux-x64/win-x64 binaries
- WCF TLS validation on Linux/WSL via `HttpClientHandler` (`POLARION_TLS_SKIP_VERIFY`, `POLARION_TLS_CA_FILE`)
- Host logging honors `POLARION_MCP_LOG_LEVEL`
- `add_work_item_comment` honors `parent_comment_id` for threaded replies

## 0.1.1

- Synced PolarionMCP dotnet branch and refreshed bundled linux-x64/win-x64 binaries

## 0.1.0

- Extracted Polarion MCP from baxter-product-owner into standalone plugin
- Cross-platform Node launcher for Linux and Windows
- Vendored PolarionMCP source under `mcp-servers/`
