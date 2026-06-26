---
name: baxter-mcp-setup
description: Diagnose Baxter plugin MCP server startup issues, uv prerequisites, and auth env vars. Use when MCP shows disconnected or failed to create client.
---

# Baxter MCP setup

## Trigger

MCP server red in Cursor/VS Code, "failed to create client", or missing tools from a Baxter plugin.

## Prerequisites (machine baseline)

These are **not** installed by the plugin. Run once per machine:

```bash
./scripts/bootstrap-dev-machine.sh
```

Requires:

- Python 3.10+
- [uv](https://docs.astral.sh/uv/) on `PATH`
- Any server-specific auth (documented in `mcp-servers/<name>/README.md`)

## Diagnosis workflow

1. Confirm the Baxter plugin is installed and MCP server is enabled in IDE settings.
2. Check IDE MCP logs (Cursor: Settings → MCP → server logs; VS Code: Output panel).
3. Run the server manually from the **installed domain plugin** (example: Polarion):

```bash
node ~/.cursor/plugins/cache/briggst5-baxterplugin/baxter-polarion/*/scripts/launch-polarion-mcp.mjs
```

For Python MCP, use that plugin's `run-mcp-server.sh` if present.

4. If `uv` is missing, run `bootstrap-dev-machine.sh`.
5. First start may take 30–60s while `uv` syncs Python dependencies.

## Common fixes

| Symptom | Fix |
|---------|-----|
| `uv: command not found` | Run bootstrap script or install uv via MDM |
| `ModuleNotFoundError` | Delete `~/.cache/uv` for that project and retry |
| Auth errors | Set env vars listed in server README |
