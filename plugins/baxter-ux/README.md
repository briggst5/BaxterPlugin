# Baxter UX / Nexus DLS

Cursor plugin for the Baxter **Nexus Design Language System** — Zero Height documentation exports, Figma reference, and scoped UI rules for web implementation.

## Contents

| Component | Location |
|-----------|----------|
| **Design tokens & catalog** | `design-system/tokens.md`, `design-system/components.md` |
| **Zero Height pages** | `design-system/pages/*.md` |
| **Web mapping** | `design-system/web-mapping.md` |
| **Figma exports** | `figma/` |
| **Rules** | `rules/` — Nexus DLS patterns + always-on authority rule |
| **Sync skill** | `skills/sync-dls` |

## Rules

| Rule | Scope |
|------|-------|
| `dls-authority` | Always on — points agents to DLS reference material |
| `nexus-dls-foundations` | `**/*.{css,cshtml}` — colors, typography, spacing |
| `nexus-dls-components` | `**/*.cshtml` — Bootstrap component patterns |
| `nexus-dls-layout` | `**/Pages/Shared/**` — layout shells and partials |
| `nexus-dls-accessibility` | `**/*.cshtml` — clinical UI accessibility |

## Prerequisites

- **Figma live access:** install the **Figma** Cursor plugin for `get_design_context` against [Nexus Web](https://www.figma.com/design/L5MvUzq1j2M8xZj8Svkdqt/Nexus-Web).
- **Consuming apps** implement tokens in project `wwwroot/css/nexus-tokens.css` (not bundled in this plugin).

## Syncing updated content

Source of truth for Zero Height exports and rules: `/mnt/d/dls` (override with `DLS_SRC`).

```bash
# From plugin root
DLS_SRC=/mnt/d/dls ./scripts/sync-dls.sh

# Install to local Cursor plugins (from BaxterPlugin repo root)
rsync -a --delete plugins/baxter-ux/ ~/.cursor/plugins/local/baxter-ux/
```

For Figma: drop exports in `figma/incoming/`, then run `sync-dls.sh`.

Use the `sync-dls` skill in Cursor for guided refresh workflows.

## Changelog

See [CHANGELOG.md](CHANGELOG.md).
