# Getting started with Baxter plugins

Baxter plugins extend **Cursor** and **VS Code GitHub Copilot** with org skills, agents, rules, and MCP servers. This guide covers install order, per-OS setup, and which plugin to use for common tasks.

## Install order

| Step | Action |
|------|--------|
| 1 | Install **baxter-core** (required for all engineers) |
| 2 | Run machine bootstrap (Python + uv) — [baxter-core INSTALL](../plugins/baxter-core/docs/INSTALL.md) |
| 3 | Install optional plugins for your role (see table below) |
| 4 | Configure MCP credentials per plugin INSTALL guide |
| 5 | Reload Cursor and enable MCP servers in Settings |

## Which plugins do I need?

| Role / need | Plugins | Install guides |
|-------------|---------|----------------|
| All engineers | **baxter-core** | [INSTALL](../plugins/baxter-core/docs/INSTALL.md) |
| PO, RTE, agile team | **baxter-product-owner** + **baxter-polarion** | [PO INSTALL](../plugins/baxter-product-owner/docs/INSTALL.md), [Polarion INSTALL](../plugins/baxter-polarion/docs/INSTALL.md) |
| Platform / embedded SW | **baxter-flc-platform-sw** | [INSTALL](../plugins/baxter-flc-platform-sw/docs/INSTALL.md) |
| Security / CVE triage | **baxter-security** | [INSTALL](../plugins/baxter-security/docs/INSTALL.md) |
| Quality / GQP compliance | **baxter-gqp** | [INSTALL](../plugins/baxter-gqp/docs/INSTALL.md) |
| Web UI (Nexus DLS) | **baxter-ux** | [INSTALL](../plugins/baxter-ux/docs/INSTALL.md) |

Org admins map plugins to SCIM groups in the Cursor Team Marketplace (Default On / Off).

## Platform setup (Linux, macOS, Windows)

Every plugin with MCP or scripts uses the same config directory pattern:

| OS | Config directory |
|----|------------------|
| Linux / macOS | `~/.config/` |
| Windows | `%USERPROFILE%\.config\` |

| Plugin | Config file | Purpose |
|--------|-------------|---------|
| baxter-product-owner | `azure-devops-mcp.env` | Azure DevOps PAT and org |
| baxter-polarion | `polarion-mcp.env` | Polarion URL, user, PAT (**90-day** expiry — renew regularly) |
| baxter-gqp | `gqp-mcp.env` | Optional non-secret GQP settings |
| baxter-security | `nvd-api.env` | NVD API key |

**Never commit credentials.** Use `chmod 600` on env files on Linux/macOS.

### Machine bootstrap (all platforms)

From BaxterPlugin repo root:

| OS | Command |
|----|---------|
| Linux / macOS | `./scripts/bootstrap-dev-machine.sh` |
| Windows | `.\scripts\bootstrap-dev-machine.ps1` |

Installs/checks **Python 3.10+** and **uv** (required for Python MCP servers).

### Node.js

Required for Azure DevOps MCP, Polarion launcher, and several setup scripts:

- **Node.js 20+** recommended
- Install from https://nodejs.org or org package manager

## Cursor marketplace install

1. Cursor → Settings → Plugins → Team Marketplace
2. Install plugins assigned to your group
3. Settings → MCP → enable servers for each domain plugin (e.g. `azure-devops`, `polarion-mcp`, `gqp-knowledge`)
4. Reload window if skills do not appear

### Local development (symlink)

```bash
mkdir -p ~/.cursor/plugins/local
for p in baxter-core baxter-product-owner baxter-polarion baxter-gqp baxter-flc-platform-sw baxter-security baxter-ux; do
  ln -sfn "$(pwd)/plugins/$p" ~/.cursor/plugins/local/$p
done
```

Windows (PowerShell):

```powershell
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\.cursor\plugins\local"
@("baxter-core","baxter-product-owner","baxter-polarion","baxter-gqp","baxter-flc-platform-sw","baxter-security","baxter-ux") | ForEach-Object {
  cmd /c mklink /J "$env:USERPROFILE\.cursor\plugins\local\$_" "$(Resolve-Path plugins\$_)"
}
```

## Example prompts by plugin

| Plugin | Try in chat |
|--------|-------------|
| baxter-core | "Review this diff using baxter standards" |
| baxter-product-owner | "Prepare sprint planning assist for Team X" |
| baxter-polarion | "List Polarion requirements for feature Y" |
| baxter-flc-platform-sw | "PR review for branch `feature/foo`" |
| baxter-security | "Is CVE-2024-1234 affecting our openssl version?" |
| baxter-gqp | "What V&V does GQP require for software changes?" |
| baxter-ux | "Implement this screen using Nexus DLS tokens" |

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| MCP server red / failed to create client | `baxter-mcp-setup` skill or [baxter-core INSTALL](../plugins/baxter-core/docs/INSTALL.md) |
| Skills not listed | Reload Cursor; confirm plugin enabled for your user |
| `uv: command not found` | Re-run bootstrap; add `~/.local/bin` to PATH |
| ADO/Polarion auth errors | Recheck env file path and PAT expiry |
| Copilot missing rules | Use `baxter-standards` skill — `.mdc` rules are Cursor-only |

## Maintainer docs

- [CONTRIBUTING.md](../CONTRIBUTING.md) — vendoring skills, MCP, version bumps
- `./scripts/validate-plugin.sh` — run before every PR
