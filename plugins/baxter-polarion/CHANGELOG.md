# Changelog

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
