---
name: sync-dls
description: Sync Baxter Nexus DLS content from the source directory and Figma exports into this plugin. Use when Zero Height pages or Figma files are updated, or when the user asks to refresh DLS reference material.
---

# Sync Nexus DLS

Refresh Zero Height exports, rules, and Figma assets in the **baxter-ux** plugin.

## Source locations

| Content | Default source | Plugin destination |
|---------|----------------|-------------------|
| Zero Height pages | `$DLS_SRC/design-system/` | `design-system/` |
| Nexus DLS rules | `$DLS_SRC/nexus-dls-*.mdc` | `rules/` |
| Figma exports | User-provided path or `figma/incoming/` | `figma/` |

Default `DLS_SRC`: `/mnt/d/dls`

## Workflow

1. Confirm the user wants to overwrite plugin reference files (read-only sync; no ADO/Polarion changes).
2. Run from the plugin root:

```bash
DLS_SRC=/mnt/d/dls ./scripts/sync-dls.sh
```

3. If Figma files were copied manually, place them under `figma/` and update `figma/README.md` with file name, export date, and Figma URL.
4. After sync, fix rule authority paths if the source still uses `docs/design-system/` — the script rewrites to `design-system/`.
5. Bump `version` in `.cursor-plugin/plugin.json` when publishing to the Baxter marketplace.
6. Re-copy to `~/.cursor/plugins/local/baxter-ux/` if the user runs from the repo:

```bash
rsync -a --delete plugins/baxter-ux/ ~/.cursor/plugins/local/baxter-ux/
```

## Validation

From the BaxterPlugin repo root:

```bash
./scripts/validate-plugin.sh
```

## Do not

- Edit Zero Height or Figma source systems directly from this skill
- Change `alwaysApply` or globs on Nexus DLS rules without PO/design review
- Commit large binary Figma files without confirming repo storage policy
