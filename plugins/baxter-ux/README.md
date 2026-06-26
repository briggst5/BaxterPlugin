# Baxter UX / Nexus DLS

Cursor plugin for the Baxter **Nexus Design Language System** — Zero Height documentation exports, Figma reference, and scoped UI rules for web implementation.

| | |
|--|--|
| **Audience** | Web UI developers, clinical application teams |
| **Install guide** | [docs/INSTALL.md](docs/INSTALL.md) (Linux, macOS, Windows) |
| **Live Figma** | [Nexus Web](https://www.figma.com/design/L5MvUzq1j2M8xZj8Svkdqt/Nexus-Web) |

## Quick start

1. Install **baxter-ux** from marketplace.
2. Optional: install **Figma** Cursor plugin for live design context.
3. In a `.cshtml` project: *"Build this table using Nexus DLS patterns"*.

No MCP or secrets required.

## What's included

### Design reference

| Resource | Location |
|----------|----------|
| Design tokens | `design-system/tokens.md` |
| Component catalog | `design-system/components.md` |
| Web adaptations | `design-system/web-mapping.md` |
| Zero Height pages | `design-system/pages/*.md` |
| Navigation index | `design-system/nav-manifest.json` |
| Figma exports | `figma/` — see [figma/README.md](figma/README.md) |

### Skills

| Skill | Use when |
|-------|----------|
| `sync-dls` | Refresh plugin from Zero Height / Figma exports |

### Rules (Cursor only)

| Rule | Scope |
|------|-------|
| `dls-authority` | Always on — points agents to DLS material |
| `nexus-dls-foundations` | `**/*.{css,cshtml}` — colors, typography, spacing |
| `nexus-dls-components` | `**/*.cshtml` — Bootstrap component patterns |
| `nexus-dls-layout` | `**/Pages/Shared/**` — layout shells |
| `nexus-dls-accessibility` | `**/*.cshtml` — clinical UI a11y |

## Implementation note

This plugin is **reference material**. Target apps implement Bootstrap + project `wwwroot/css/nexus-tokens.css` — tokens are not injected automatically.

## Figma workflow

1. Install Figma Cursor plugin.
2. Share Nexus Web URL or component link in chat.
3. Agent uses `get_design_context` + local `design-system/` for implementation.

## Syncing content (maintainers)

```bash
DLS_SRC=/path/to/export ./scripts/sync-dls.sh
```

See [INSTALL.md](docs/INSTALL.md) for per-OS paths and local plugin install.

## Example prompts

- "Use Nexus tokens for primary button and spacing on this page"
- "Review this Razor view for DLS accessibility requirements"
- "Sync DLS from source" (`sync-dls` skill)

## Changelog

See [CHANGELOG.md](CHANGELOG.md).
