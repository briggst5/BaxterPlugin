# Baxter Core

Required baseline plugin for all Baxter engineers.

## Contents

- **Skills:** `baxter-standards`, `baxter-mcp-setup`
- **Rules (Cursor only):** engineering standards, plugin quality gates
- **Agents:** `baxter-code-reviewer`
- **MCP:** `baxter-echo` (connectivity example; replace by copying real MCP projects into `mcp-servers/`)

## Machine prerequisites

Plugin install does **not** install Python or `uv`. Run once per machine:

```bash
../../scripts/bootstrap-dev-machine.sh
```

| Prerequisite | Purpose |
|--------------|---------|
| Python 3.10+ | MCP runtime |
| `uv` on PATH | Lazy `pip` sync on first MCP start |

## MCP servers

| Server | Auth | First-start behavior |
|--------|------|----------------------|
| `baxter-echo` | None | `uv sync` in `mcp-servers/baxter-echo/` |

Manual test:

```bash
./scripts/run-mcp-server.sh baxter-echo
```

Copy real MCP projects into `mcp-servers/<name>/` and register with `./scripts/add-mcp-entry.sh` (see repo [CONTRIBUTING.md](../../CONTRIBUTING.md)).

## Cursor vs Copilot

| Component | Cursor manifest | Copilot manifest |
|-----------|-----------------|------------------|
| Skills, agents | `.cursor-plugin/plugin.json` | `.plugin/plugin.json` |
| MCP config | `.mcp.json` (`${CURSOR_PLUGIN_ROOT}`) | `.mcp.copilot.json` (`${PLUGIN_ROOT}`) |
| Rules | `rules/*.mdc` | Not supported — use `baxter-standards` skill |

## Changelog

See [CHANGELOG.md](CHANGELOG.md).
