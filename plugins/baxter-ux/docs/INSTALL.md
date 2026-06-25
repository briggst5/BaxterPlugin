# Baxter UX / Nexus DLS — Installation

Nexus Design Language System reference for Cursor — tokens, components, Zero Height exports, and scoped UI rules.

## Prerequisites

| Requirement | Purpose |
|-------------|---------|
| **baxter-ux** plugin | From marketplace |
| **Figma plugin** (optional) | Live `get_design_context` from [Nexus Web Figma](https://www.figma.com/design/L5MvUzq1j2M8xZj8Svkdqt/Nexus-Web) |
| **DLS source** (maintainers) | `DLS_SRC` for `sync-dls.sh` — typically internal Zero Height export path |

Consuming applications implement tokens in project `wwwroot/css/nexus-tokens.css` — not bundled in this plugin.

## Step 1 — Install plugins

1. Install **baxter-ux** from marketplace.
2. Optional: install **Figma** Cursor plugin for design-to-code against Nexus Web.
3. Reload Cursor.

No MCP or credential file required for read-only DLS reference.

## Step 2 — Verify rules apply

Open a `.cshtml` or CSS file in a web project. Rules auto-apply by file pattern:

| Rule | Files |
|------|-------|
| `nexus-dls-foundations` | `**/*.{css,cshtml}` |
| `nexus-dls-components` | `**/*.cshtml` |
| `nexus-dls-layout` | `**/Pages/Shared/**` |
| `nexus-dls-accessibility` | `**/*.cshtml` |

In chat: *"Implement this form using Nexus DLS components and tokens"*.

## Step 3 — Sync updated DLS content (maintainers)

Source of truth for exports is typically an internal path (override with `DLS_SRC`).

### Linux / macOS

```bash
cd plugins/baxter-ux
DLS_SRC=/path/to/dls-export ./scripts/sync-dls.sh

# Install to local Cursor plugins (from BaxterPlugin root)
rsync -a --delete plugins/baxter-ux/ ~/.cursor/plugins/local/baxter-ux/
```

### Windows (Git Bash or WSL)

```bash
cd plugins/baxter-ux
DLS_SRC=/mnt/d/dls ./scripts/sync-dls.sh
```

For Figma: drop exports in `figma/incoming/`, then run `sync-dls.sh`.

Use the `sync-dls` skill in Cursor for guided refresh.

### Windows (PowerShell — copy to local plugins)

```powershell
$src = Resolve-Path plugins\baxter-ux
$dst = "$env:USERPROFILE\.cursor\plugins\local\baxter-ux"
New-Item -ItemType Directory -Force -Path (Split-Path $dst)
cmd /c mklink /J $dst $src
```

## WSL + Windows paths

If `DLS_SRC` is on a Windows drive from WSL, use `/mnt/d/...` paths. Run sync from the environment where `DLS_SRC` is accessible.

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Agent ignores DLS | Confirm `dls-authority` rule loads; mention "Nexus DLS" in prompt |
| Stale tokens | Run `sync-dls` skill or `sync-dls.sh` |
| Figma context empty | Install Figma plugin; share figma.com URL in chat |
| Tokens not in app | Copy patterns to project `nexus-tokens.css` — plugin is reference only |

## Related

- [README.md](../README.md)
- [design-system/tokens.md](../design-system/tokens.md)
- [figma/README.md](../figma/README.md)
