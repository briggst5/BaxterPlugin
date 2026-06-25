# Baxter Core

**Required** baseline plugin for all Baxter engineers — org standards, MCP troubleshooting, and shared code review agent.

| | |
|--|--|
| **Audience** | Every engineer |
| **Cursor mode** | Required |
| **Install guide** | [docs/INSTALL.md](docs/INSTALL.md) (Linux, macOS, Windows) |

## Quick start

1. Install **Baxter Core** from the team marketplace.
2. Run machine bootstrap from BaxterPlugin root: `./scripts/bootstrap-dev-machine.sh` (or `.ps1` on Windows).
3. Enable **baxter-echo** MCP in Cursor Settings → MCP to verify connectivity.

## What's included

### Skills

| Skill | Use when |
|-------|----------|
| `baxter-standards` | Every implementation or review — minimal diffs, conventions, no secrets, meaningful tests |
| `baxter-mcp-setup` | MCP server disconnected, `uv` missing, or auth env issues |

### Agents

| Agent | Use when |
|-------|----------|
| `baxter-code-reviewer` | Pre-PR review for Baxter scope, tests, and security basics |

### Rules (Cursor only)

| Rule | Scope |
|------|-------|
| Engineering standards | Always on — diff discipline, imports, TypeScript exhaustive switch |
| Plugin quality gates | Plugin authoring — manifests, paths, frontmatter |

Copilot users: invoke `baxter-standards` skill — `.mdc` rules are not supported in Copilot.

### MCP servers

| Server | Auth | Purpose |
|--------|------|---------|
| `baxter-echo` | None | Connectivity example; first-start `uv sync` |

Manual test: `./scripts/run-mcp-server.sh baxter-echo`

Add production MCP by copying projects into `mcp-servers/` and registering via `./scripts/add-mcp-entry.sh` — see [CONTRIBUTING.md](../../CONTRIBUTING.md).

## Machine prerequisites

Plugin install does **not** install Python or `uv`. Run once per machine from BaxterPlugin root:

| OS | Command |
|----|---------|
| Linux / macOS | `./scripts/bootstrap-dev-machine.sh` |
| Windows | `.\scripts\bootstrap-dev-machine.ps1` |

| Prerequisite | Purpose |
|--------------|---------|
| Python 3.10+ | MCP runtime |
| `uv` on PATH | Lazy pip sync on first MCP start |

## Related plugins

Domain capabilities live in optional plugins — install what your role needs:

| Plugin | For |
|--------|-----|
| baxter-product-owner | SAFe / ADO |
| baxter-polarion | Polarion MCP |
| baxter-flc-platform-sw | Code / PR review checklists |
| baxter-security | CVE / security review |
| baxter-gqp | GQP compliance |
| baxter-ux | Nexus DLS |

See [getting started](../../docs/getting-started.md) for the full matrix.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).
