# baxter-echo MCP

Example Python MCP bundled with `baxter-core` to validate the plugin + `uv run` launch pattern.

Example Python MCP vendored in `baxter-core` to validate the plugin + `uv run` launch pattern.

Copy your real MCP repos into `mcp-servers/<name>/` in this plugin (see [CONTRIBUTING.md](../../../CONTRIBUTING.md)).

## Prerequisites

- Python 3.10+
- `uv` on PATH (install via `scripts/bootstrap-dev-machine.sh`)

Plugin install does **not** install Python deps. First MCP start runs `uv sync` lazily.

## Auth

None required for the echo server.

## Manual test

From the plugin root:

```bash
./scripts/run-mcp-server.sh baxter-echo
```

## Tools

- `echo` — connectivity test
- `health` — returns `ok`
