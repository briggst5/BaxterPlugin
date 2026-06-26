# Baxter Polarion

Standalone plugin for the vendored **Polarion MCP** server — Linux, Windows, and macOS, bundled binaries, no end-user compile.

| | |
|--|--|
| **Audience** | Teams using Polarion for requirements and compliance |
| **Install guide** | [docs/INSTALL.md](docs/INSTALL.md) (Linux, macOS, Windows) |
| **Pair with** | **baxter-product-owner** for PO/RTE skills and traceability agents |

## Quick start

```bash
node plugins/baxter-polarion/scripts/setup-polarion-env.mjs
# Edit ~/.config/polarion-mcp.env — POLARION_URL, POLARION_USER, POLARION_PAT, POLARION_PROJECT
```

Enable **polarion-mcp** in Cursor → Settings → MCP.

## What's included

### MCP server

| Component | Location |
|-----------|----------|
| Polarion MCP source | `mcp-servers/` |
| Launcher | `scripts/launch-polarion-mcp.mjs` |
| Linux binary | `bin/linux-x64/polarion-mcp` |
| Windows binary | `bin/win-x64/polarion-mcp.exe` |
| macOS Intel binary | `bin/osx-x64/polarion-mcp` |
| macOS Apple Silicon binary | `bin/osx-arm64/polarion-mcp` |

Server starts immediately from the bundled executable.

### Used by (baxter-product-owner)

| Skill / agent | Purpose |
|---------------|---------|
| `polarion-requirements-steward` | SRS, LiveDocs, reviews |
| `traceability-auditor` agent | Polarion ↔ ADO link audits |
| IEC 62304 workflows (baxter-flc-platform-sw) | Requirements in Polarion |

## Prerequisites

- **Node.js 20+** (launcher)
- Polarion URL, user, and PAT — [INSTALL.md](docs/INSTALL.md)

## Configuration

`~/.config/polarion-mcp.env` (Windows: `%USERPROFILE%\.config\polarion-mcp.env`)

Template: `mcp-servers/polarion-mcp.env.example`

**PAT setup:** [docs/INSTALL.md](docs/INSTALL.md#step-2b--generate-a-polarion-personal-access-token-pat) — generate in Polarion **My Account → Personal Access Token**. PATs expire after **90 days**; renew before expiry to avoid MCP auth failures.

## Operations

Detailed runbook: [mcp-servers/docs/OPERATIONS.md](mcp-servers/docs/OPERATIONS.md)

## Maintainers

| Task | Command |
|------|---------|
| Sync upstream | `./scripts/sync-polarion-dotnet.sh` |
| Rebuild binaries | `./scripts/build-polarion-binaries.sh` |
| CI sync workflow | `.github/workflows/sync-polarion-dotnet.yml` |

Bump version in both manifest files and `CHANGELOG.md` on release.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).
